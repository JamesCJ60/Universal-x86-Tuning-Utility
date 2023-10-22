using Accord.Math.Distances;
using craftersmine.SteamGridDBNet;
using GameLib.Plugin.RiotGames.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    class GetImages
    {
        public static async Task<string> GetIconImageUrl(string gameName)
        {
            try
            {
                string path = Universal_x86_Tuning_Utility.Properties.Settings.Default.Path;
                if (!File.Exists(path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg"))
                {

                    var client = new SteamGridDb("33006ae9737e547251b1cff96e9e6ec9");
                    SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

                    // Instantiate the string distance algorithm
                    var levenshtein = new Levenshtein();
                    var iconSizeThresholds = new[] { 256, 192, 128, 512, 1024, 96 };

                    foreach (var result in games)
                    {
                        double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                        if (distance <= 3)
                        {
                            SteamGridDbIcon[]? icons = await client.GetIconsByGameIdAsync(result.Id);

                            // Use Task.WhenAny to fetch icons in parallel and return the first matching one
                            var iconTasks = icons
                                .Where(hero => iconSizeThresholds.Any(size => hero.Width >= size))
                                .Select(async hero =>
                                {
                                    var iconUrl = hero.FullImageUrl;
                                    if (!string.IsNullOrEmpty(iconUrl))
                                    {
                                        return iconUrl;
                                    }
                                    return null;
                                })
                                .ToList(); // Store the tasks in a list

                            while (iconTasks.Any())
                            {
                                var completedTask = await Task.WhenAny(iconTasks);
                                iconTasks.Remove(completedTask); // Remove the completed task after awaiting it

                                if (completedTask.Result != null)
                                {
                                    string filePath = path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg";
                                    await DownloadImage(completedTask.Result, filePath);
                                    return filePath;
                                }
                            }
                        }
                    }

                }
                else return path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg";

                return await GetGridImageUrl(gameName);
            }
            catch
            {
                return await GetGridImageUrl(gameName);
            }
        }

        public static async Task<string> GetGridImageUrl(string gameName)
        {
            string path = Universal_x86_Tuning_Utility.Properties.Settings.Default.Path;
            try
            {
                if (!File.Exists(path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg"))
                {

                    var client = new SteamGridDb("33006ae9737e547251b1cff96e9e6ec9");
                    SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

                    // Instantiate the string distance algorithm
                    var levenshtein = new Levenshtein();
                    var iconSizeThresholds = new[] { 1024, 512 };

                    foreach (var result in games)
                    {
                        double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                        if (distance <= 3)
                        {
                            SteamGridDbGrid[]? icons = await client.GetGridsByGameIdAsync(result.Id);

                            // Use Task.WhenAny to fetch icons in parallel and return the first matching one
                            var iconTasks = icons
                                .Where(hero => iconSizeThresholds.Any(size => hero.Width == size))
                                .Select(async hero =>
                                {
                                    var iconUrl = hero.FullImageUrl;
                                    if (!string.IsNullOrEmpty(iconUrl))
                                    {
                                        return iconUrl;
                                    }
                                    return null;
                                })
                                .ToList(); // Store the tasks in a list

                            while (iconTasks.Any())
                            {
                                var completedTask = await Task.WhenAny(iconTasks);
                                iconTasks.Remove(completedTask); // Remove the completed task after awaiting it

                                if (completedTask.Result != null)
                                {
                                    string filePath = path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg";
                                    await DownloadImage(completedTask.Result, filePath);
                                    return filePath;
                                }
                            }
                        }
                    }

                }
                else return path + $"\\Assets\\GameImages\\{CleanFileName(gameName)}.jpeg";

            }
            catch { }
            return path + "\\Assets\\GameImages\\default.png";
        }

        public static string CleanFileName(string input)
        {
            string illegalChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            string pattern = "[" + Regex.Escape(illegalChars) + "]";
            return Regex.Replace(input, pattern, "_");
        }


        public static async Task DownloadImage(string url, string filePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }
}
