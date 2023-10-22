using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using GameLib;
using GameLib.Core;
using System.Windows;
using System.Threading;
using System.Xml.Linq;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using System.Drawing.Drawing2D;
using Windows.System;
using System.Windows.Documents;
using System.Reflection;
using Windows.Gaming.Preview.GamesEnumeration;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;
using GameLib.Plugin.RiotGames.Model;
using System.Linq.Expressions;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;
using Universal_x86_Tuning_Utility.Views.Pages;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;

namespace Universal_x86_Tuning_Utility.Scripts
{
    internal class Game_Manager
    {
        public class GameLauncherItem
        {
            public string gameID { get; set; }
            public string gameName { get; set; }
            public string appType { get; set; }
            public string launchCommand { get; set; }
            public string path { get; set; }
            public string exe { get; set; }
            public string imageLocation { get; set; } = "";
            public string iconPath { get; set; } = "";
        }

        public static List<GameLauncherItem> installedGames = null;

        public static List<GameLauncherItem> syncGame_Library(bool isAdaptive = false)
        {
            try
            {
                List<GameLauncherItem> list = new List<GameLauncherItem>();
                //gamelauncher
                LauncherManager gameLauncher = new LauncherManager(new GameLib.Core.LauncherOptions() { QueryOnlineData = false });

                GameLauncherItem extraApps = new GameLauncherItem();

                foreach (var launcher in gameLauncher.GetLaunchers())
                {
                    switch (launcher.Name)
                    {
                        case "Steam":
                            foreach (var game in launcher.Games)
                            {
                                if (!game.Name.Contains("Steamworks") && !game.Name.Contains("SteamVR") && !game.Name.Contains("Google Earth") && !game.Name.Contains("Wallpaper Engine") && !game.Name.Contains("tModLoader") && !game.Name.Contains("- Original Soundtrack"))
                                {
                                    if (game.Id != "228980")
                                    {
                                        GameLauncherItem launcherItem = new GameLauncherItem();
                                        launcherItem.gameName = game.Name;
                                        launcherItem.gameID = game.Id;
                                        launcherItem.launchCommand = $"{launcher.Name}-{game.LaunchString}-{game.Id}-{game.Name}";
                                        //launcherItem.iconPath = game.ExecutableIcon;

                                        if (game.Executables.Count() == 1)
                                        {
                                            launcherItem.path = game.InstallDir;
                                            launcherItem.exe = Path.GetFileNameWithoutExtension(game.Executables.First());
                                        }
                                        else
                                        {

                                            string[] array = launcherItem.gameName.Split(' ');
                                            foreach (string exe in game.Executables)
                                            {

                                                string exeName = Path.GetFileNameWithoutExtension(exe);
                                                if (game.Name.Contains("Call of duty", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (exeName.Contains("cod", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        launcherItem.path = game.InstallDir;
                                                        launcherItem.exe = exeName;
                                                        break;
                                                    }
                                                }
                                                foreach (string arr in array)
                                                {
                                                    if (exeName.Contains(arr, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        launcherItem.path = game.InstallDir;
                                                        launcherItem.exe = exeName;
                                                        break;
                                                    }
                                                }
                                                if (launcherItem.path != null) { break; }
                                            }
                                        }
                                        if (launcherItem.path == "" || launcherItem.exe == "")
                                        {
                                            launcherItem.path = game.InstallDir;
                                            launcherItem.exe = Path.GetFileNameWithoutExtension(game.Executables.Last());
                                        }


                                        launcherItem.appType = launcher.Name;
                                        list.Add(launcherItem);
                                    }

                                }
                            }
                            break;
                        case "Battle.net":
                            foreach (var game in launcher.Games)
                            {
                                GameLauncherItem launcherItem = new GameLauncherItem();
                                launcherItem.gameName = game.Name;
                                launcherItem.gameID = game.Id;
                                launcherItem.launchCommand = $"{launcher.Name}-{game.LaunchString}-{game.Id}-{game.Name}";
                                //launcherItem.iconPath = game.ExecutableIcon;
                                switch (game.Name)
                                {
                                    case "Call of Duty Black Ops Cold War":
                                        launcherItem.path = game.InstallDir;
                                        launcherItem.exe = "BlackOpsColdWar";
                                        break;

                                    default:
                                        launcherItem.path = game.InstallDir;
                                        launcherItem.exe = Path.GetFileNameWithoutExtension(launcherItem.path);
                                        break;
                                }

                                launcherItem.appType = launcher.Name;
                                list.Add(launcherItem);



                            }
                            break;
                        case "Epic Games":
                            foreach (var game in launcher.Games)
                            {
                                GameLauncherItem launcherItem = new GameLauncherItem();
                                launcherItem.gameName = game.Name;
                                launcherItem.gameID = game.Id;
                                launcherItem.launchCommand = $"{launcher.Name}-{game.LaunchString}-{game.Id}-{game.Name}";
                                launcherItem.path = game.InstallDir;
                                //launcherItem.iconPath = game.ExecutableIcon;
                                launcherItem.exe = Path.GetFileNameWithoutExtension(launcherItem.path);
                                launcherItem.appType = launcher.Name;
                                list.Add(launcherItem);

                            }
                            break;

                        default:
                            foreach (var game in launcher.Games)
                            {
                                GameLauncherItem launcherItem = new GameLauncherItem();
                                launcherItem.gameName = game.Name;
                                launcherItem.gameID = game.Id;
                                launcherItem.launchCommand = $"{launcher.Name}-{game.LaunchString}-{game.Id}-{game.Name}";
                                launcherItem.path = game.InstallDir;
                                launcherItem.exe = Path.GetFileNameWithoutExtension(launcherItem.path);
                                launcherItem.appType = launcher.Name;
                                //launcherItem.iconPath = game.ExecutableIcon;
                                list.Add(launcherItem);
                            }
                            break;

                    }

                }

                //microsoft store apps below

                PackageManager packageManager = new PackageManager();
                IEnumerable<Windows.ApplicationModel.Package> packages = packageManager.FindPackages();

                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    try
                    {
                        string xboxGameDirectory = Path.Combine(d.Name, "XboxGames");
                        string[] filesInDirectory;
                        if (Directory.Exists(xboxGameDirectory))
                        {
                            filesInDirectory = Directory.GetDirectories(xboxGameDirectory);

                            if (filesInDirectory.Length > 0)
                            {
                                string[] strings = filesInDirectory.Select(x => Path.GetFileName(x)).ToArray();

                                if (strings.Length > 0)
                                {
                                    foreach (Package package in packages)
                                    {
                                        string install = package.InstalledLocation.Path;
                                        string sig = package.SignatureKind.ToString();

                                        if (install.Contains("WindowsApps") && sig == "Store" && package.IsFramework == false)
                                        {
                                            if (strings.Contains(package.DisplayName))
                                            {
                                                GameLauncherItem launcherItem = new GameLauncherItem();
                                                launcherItem.gameName = package.DisplayName;
                                                launcherItem.gameID = package.Id.FullName;
                                                launcherItem.launchCommand = $"Microsoft Store-{package.Id.FullName}-{package.DisplayName}";
                                                launcherItem.path = package.InstalledPath;
                                                //launcherItem.exe = Path.GetFileNameWithoutExtension(launcherItem.path);
                                                launcherItem.appType = "Microsoft Store";
                                                launcherItem.imageLocation = package.Logo.AbsolutePath;
                                                list.Add(launcherItem);

                                            }

                                        }

                                    }
                                }

                            }
                        }
                    }
                    catch { }
                }

                list = list.OrderBy(item => item.gameName).ToList();

                if (isAdaptive)
                {
                    extraApps = new GameLauncherItem();
                    extraApps.gameName = "Yuzu";
                    extraApps.path = "yuzu.exe";
                    list.Add(extraApps);

                    extraApps = new GameLauncherItem();
                    extraApps.gameName = "RPCS3";
                    extraApps.path = "rpcs3.exe";
                    list.Add(extraApps);

                    extraApps = new GameLauncherItem();
                    extraApps.gameName = "Cemu";
                    extraApps.path = "cemu.exe";
                    list.Add(extraApps);

                    extraApps = new GameLauncherItem();
                    extraApps.gameName = "Dolphin";
                    extraApps.path = "Dolphin.exe";
                    list.Add(extraApps);

                    extraApps = new GameLauncherItem();
                    extraApps.gameName = "Citra";
                    extraApps.path = "Citra.exe";
                    list.Add(extraApps);
                }

                var distinctGameLauncherItems = list.Distinct(new GameLauncherItemEqualityComparer()).ToList();
                return distinctGameLauncherItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static void LaunchApp(string gameID, string appType, string launchcommand, string appLocation)
        {
            if (appType == "Exe")
            {
                if (File.Exists(appLocation))
                {
                    RunGame(appLocation);
                }

            }
            else
            {
                if (gameID != "")
                {
                    switch (appType)
                    {
                        case "Epic Games":
                            RunLaunchString(launchcommand);
                            break;
                        case "Steam":
                            RunLaunchString(launchcommand);
                            break;
                        case "Battle.net":
                            string battlenetfile = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "Battle.net\\Battle.net.exe");
                            if (BattleNetRunning())
                            {
                                Run_CLI.RunCommand(" --exec=\"launch " + gameID.ToUpper() + "\"", false, battlenetfile, 3000, true);
                            }
                            else
                            {

                                RunGame(battlenetfile);
                                Thread.Sleep(15000);
                                Run_CLI.RunCommand(" --exec=\"launch " + gameID.ToUpper() + "\"", false, battlenetfile, 3000, true);

                            }

                            break;
                        case "GOG Galaxy":
                            Run_CLI.RunCommand(" /command=runGame /gameId=" + gameID, false, Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "GOG Galaxy", "GalaxyClient.exe"));

                            break;
                        case "Microsoft Store":
                            PackageManager pm = new PackageManager();
                            pm.FindPackage(gameID).GetAppListEntries().First().LaunchAsync();
                            pm = null;
                            break;
                        default: break;
                    }
                }

            }

        }

        private static void RunGame(string command)
        {
            try
            {
                if (File.Exists(command))
                {

                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = Path.GetFileName(command),
                        WorkingDirectory = Path.GetDirectoryName(command)
                    });
                }
            }
            catch { /* ignore */ }
        }


        public static void RunLaunchString(string command)
        {

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = command
                };
                System.Diagnostics.Process.Start(psi);

            }
            catch { /* ignore */ }
        }

        public static bool BattleNetRunning()
        {
            Process[] pname = Process.GetProcessesByName("Battle.net.exe");
            if (pname.Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public class GameLauncherItemEqualityComparer : IEqualityComparer<GameLauncherItem>
        {
            public bool Equals(GameLauncherItem x, GameLauncherItem y)
            {
                // Check if two GameLauncherItems are equal based on gameID, gameName, and appType.
                return x.gameID == y.gameID && x.gameName == y.gameName && x.appType == y.appType;
            }

            public int GetHashCode(GameLauncherItem obj)
            {
                // Generate a hash code based on gameID, gameName, and appType.
                return (obj.gameID + obj.gameName + obj.appType).GetHashCode();
            }
        }

    }
}
