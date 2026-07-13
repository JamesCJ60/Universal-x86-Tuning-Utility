using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Universal_x86_Tuning_Utility.Services
{
    public sealed record LanguageOption(string CultureName, string DisplayName);

    public static class LocalizationService
    {
        private static readonly IReadOnlyList<LanguageOption> Languages = new LanguageOption[]
        {
            new("en-GB", "English"),
            new("cy-GB", "Cymraeg"),
            new("zh-CN", "简体中文"),
            new("ja-JP", "日本語"),
            new("uk-UA", "Українська"),
            new("de-DE", "Deutsch"),
            new("tr-TR", "Türkçe"),
            new("id-ID", "Bahasa Indonesia"),
            new("pt-BR", "Português (Brasil)"),
            new("fr-FR", "Français"),
            new("es-ES", "Español"),
            new("nl-NL", "Nederlands"),
            new("pt-PT", "Português"),
            new("ru-RU", "Русский"),
            new("it-IT", "Italiano"),
            new("pl-PL", "Polski")
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Catalogues = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConditionalWeakTable<object, Dictionary<string, string>> Sources = new();
        private static readonly ConditionalWeakTable<object, TrackingMarker> TrackingMarkers = new();
        private static readonly List<WeakReference<object>> TrackedElements = new();
        private static readonly object TrackingSync = new();
        private static readonly string[] StringProperties = { "Text", "Content", "Header", "Title", "Message", "ToolTip", "PlaceholderText" };
        private static bool _isInitialised;

        public static IReadOnlyList<LanguageOption> SupportedLanguages => Languages;
        public static string CurrentCultureName { get; private set; } = "en-GB";
        public static event EventHandler? CultureChanged;

        public static void Initialize(string? cultureName)
        {
            if (!_isInitialised)
            {
                EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler(ElementLoaded), true);
                EventManager.RegisterClassHandler(typeof(FrameworkContentElement), FrameworkContentElement.LoadedEvent, new RoutedEventHandler(ElementLoaded), true);
                _isInitialised = true;
            }

            SetCulture(cultureName, false);
        }

        public static void SetCulture(string? cultureName, bool refreshWindows = true)
        {
            var selected = Languages.FirstOrDefault(language => string.Equals(language.CultureName, cultureName, StringComparison.OrdinalIgnoreCase)) ?? Languages[0];
            CurrentCultureName = selected.CultureName;
            var culture = CultureInfo.GetCultureInfo(selected.CultureName);
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            LoadCatalogue("en-GB");
            LoadCatalogue(CurrentCultureName);

            if (!refreshWindows || Application.Current == null)
            {
                CultureChanged?.Invoke(null, EventArgs.Empty);
                return;
            }

            RefreshTrackedElements();

            foreach (Window window in Application.Current.Windows)
            {
                ApplyTree(window, new HashSet<object>(ReferenceEqualityComparer.Instance));
            }

            CultureChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string Get(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            if (Catalogues.TryGetValue(CurrentCultureName, out var current) && current.TryGetValue(source, out var translated))
            {
                return translated;
            }

            if (Catalogues.TryGetValue("en-GB", out var english) && english.TryGetValue(source, out var englishText))
            {
                if (Catalogues.TryGetValue(CurrentCultureName, out current) && current.TryGetValue(englishText, out translated))
                {
                    return translated;
                }

                return englishText;
            }

            if (!source.Contains("{0}", StringComparison.Ordinal) && source.StartsWith("Your preset ", StringComparison.Ordinal) && source.EndsWith(" has been saved successfully!", StringComparison.Ordinal))
            {
                return Format("Your preset {0} has been saved successfully!", source[12..^29]);
            }

            if (!source.Contains("{0}", StringComparison.Ordinal) && source.StartsWith("Your preset ", StringComparison.Ordinal) && source.EndsWith(" has been deleted successfully!", StringComparison.Ordinal))
            {
                return Format("Your preset {0} has been deleted successfully!", source[12..^31]);
            }

            if (!source.Contains("{0}", StringComparison.Ordinal) && source.StartsWith("Launching ", StringComparison.Ordinal))
            {
                return Format("Launching {0}", source[10..]);
            }

            return source;
        }

        public static string Format(string source, params object?[] arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, Get(source), arguments);
        }

        private static void ElementLoaded(object sender, RoutedEventArgs e)
        {
            TrackElement(sender);
            ApplyElement(sender);

            if (sender is Page or Window)
            {
                ((DispatcherObject)sender).Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    ApplyTree(sender, new HashSet<object>(ReferenceEqualityComparer.Instance))));
            }
        }

        private static void TrackElement(object element)
        {
            lock (TrackingSync)
            {
                if (TrackingMarkers.TryGetValue(element, out _))
                {
                    return;
                }

                TrackingMarkers.Add(element, new TrackingMarker());
                TrackedElements.Add(new WeakReference<object>(element));

                if (TrackedElements.Count > 4096)
                {
                    TrackedElements.RemoveAll(reference => !reference.TryGetTarget(out _));
                }
            }
        }

        private static void RefreshTrackedElements()
        {
            List<object> elements = new();
            lock (TrackingSync)
            {
                for (var index = TrackedElements.Count - 1; index >= 0; index--)
                {
                    if (TrackedElements[index].TryGetTarget(out var element))
                    {
                        elements.Add(element);
                    }
                    else
                    {
                        TrackedElements.RemoveAt(index);
                    }
                }
            }

            foreach (var element in elements)
            {
                ApplyElement(element);
            }
        }

        private static void ApplyTree(object element, HashSet<object> visited)
        {
            if (!visited.Add(element))
            {
                return;
            }

            TrackElement(element);
            ApplyElement(element);

            if (element is DependencyObject dependencyObject)
            {
                var visualCount = dependencyObject is Visual or Visual3D ? VisualTreeHelper.GetChildrenCount(dependencyObject) : 0;
                for (var index = 0; index < visualCount; index++)
                {
                    ApplyTree(VisualTreeHelper.GetChild(dependencyObject, index), visited);
                }

                foreach (var child in LogicalTreeHelper.GetChildren(dependencyObject).OfType<object>())
                {
                    ApplyTree(child, visited);
                }
            }
        }

        private static void ApplyElement(object element)
        {
            if (element is FrameworkElement { TemplatedParent: not null })
            {
                return;
            }

            foreach (var propertyName in StringProperties)
            {
                ApplyProperty(element, propertyName);
            }
        }

        private static void ApplyProperty(object element, string propertyName)
        {
            if (propertyName == "Text")
            {
                if (element is TextBlock textBlock)
                    ApplyDependencyProperty(textBlock, TextBlock.TextProperty, propertyName);
                else if (element is Run run)
                    ApplyDependencyProperty(run, Run.TextProperty, propertyName);
                else if (element is AccessText accessText)
                    ApplyDependencyProperty(accessText, AccessText.TextProperty, propertyName);
                return;
            }

            var property = element.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || (property.PropertyType != typeof(string) && property.PropertyType != typeof(object)) || !property.CanRead || !property.CanWrite || property.GetIndexParameters().Length != 0)
            {
                return;
            }

            if (element is DependencyObject dependencyObject)
            {
                var descriptor = DependencyPropertyDescriptor.FromName(propertyName, element.GetType(), element.GetType());
                if (descriptor != null && BindingOperations.IsDataBound(dependencyObject, descriptor.DependencyProperty))
                {
                    return;
                }
            }

            string? value;
            try
            {
                value = property.GetValue(element) as string;
            }
            catch
            {
                return;
            }

            var values = Sources.GetOrCreateValue(element);
            if (!values.TryGetValue(propertyName, out var source))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                source = FindSource(value);
                values[propertyName] = source;
            }

            var translated = Get(source);
            if (string.Equals(value, translated, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                property.SetValue(element, translated);
            }
            catch
            {
            }
        }

        private static void ApplyDependencyProperty(DependencyObject element, DependencyProperty property, string propertyName)
        {
            if (BindingOperations.IsDataBound(element, property))
            {
                return;
            }

            if (element.GetValue(property) is not string value || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var values = Sources.GetOrCreateValue(element);
            if (!values.TryGetValue(propertyName, out var source))
            {
                source = FindSource(value);
                values[propertyName] = source;
            }

            var translated = Get(source);
            if (!string.Equals(value, translated, StringComparison.Ordinal))
            {
                element.SetCurrentValue(property, translated);
            }
        }

        private static void LoadCatalogue(string cultureName)
        {
            if (Catalogues.ContainsKey(cultureName))
            {
                return;
            }

            var resourceName = $"Universal_x86_Tuning_Utility.Localization.{cultureName}.json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Catalogues[cultureName] = new Dictionary<string, string>(StringComparer.Ordinal);
                return;
            }

            using var reader = new StreamReader(stream);
            Catalogues[cultureName] = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd()) ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        private static string FindSource(string value)
        {
            foreach (var catalogue in Catalogues.Values)
            {
                foreach (var entry in catalogue)
                {
                    if (!string.Equals(entry.Value, value, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (Catalogues.TryGetValue("en-GB", out var english))
                    {
                        foreach (var englishEntry in english)
                        {
                            if (string.Equals(englishEntry.Value, entry.Key, StringComparison.Ordinal))
                            {
                                return englishEntry.Key;
                            }
                        }
                    }

                    return entry.Key;
                }
            }

            return value;
        }

        private sealed class TrackingMarker
        {
        }
    }
}
