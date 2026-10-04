using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universal_x86_Tuning_Utility.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Universal_x86_Tuning_Utility.Views.Pages;

public partial class CustomPresets : INavigableView<CustomPresetsViewModel>
{
    public CustomPresetsViewModel ViewModel { get; }

    public CustomPresets(CustomPresetsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await ViewModel.ActivateAsync();
        Unloaded += (_, _) => ViewModel.Deactivate();
    }
    private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
    {
        // Mark event as handled
        e.Handled = true;
    }
    private void mainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(15, 0, -12, 0);
        else mainCon.Margin = new Thickness(15, 0, 0, 0);
    }
    public bool IsScrollBarVisible(ScrollViewer scrollViewer)
    {
        if (scrollViewer == null) throw new ArgumentNullException(nameof(scrollViewer));

        return scrollViewer.ExtentHeight > scrollViewer.ViewportHeight;
    }

}
