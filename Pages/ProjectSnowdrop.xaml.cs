using UXTU.Properties;
using AATUV3.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using UXTU.Scripts.SMU_Backend_Scripts;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Runtime.InteropServices;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class ProjectSnowdrop : System.Windows.Controls.UserControl
    {
        string path = Settings.Default["Path"].ToString();
        string apuname = (string)Settings.Default["CPUName"];
        string APUPresetToLoad = "";
        string preset1 = "";
        string preset2 = "";
        string preset3 = "";
        //string preset4 = "";

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        const int ENUM_CURRENT_SETTINGS = -1;

        public ProjectSnowdrop()
        {
            InitializeComponent();
            try
            {
                string apuPreset = path + "\\project-snowdrop\\APU";
                string devicePath = path + "\\project-snowdrop\\Device";

                if (File.Exists(devicePath + "\\config.txt"))
                {
                    imgDevice.Source = new BitmapImage(new Uri(devicePath + "\\config.png"));

                    var lines = File.ReadAllLines(devicePath + $"\\config.txt");

                    lblPresetName.Text = lines[1];
                    lblCPU.Text = lines[4];
                    lblGPU.Text = lines[5];
                    lblRAM.Text = lines[6];
                    lblStore.Text = lines[7];
                    lblDisplay.Text = lines[8];

                    if (lines[11].Contains("N/A"))
                    {
                        btnPreset1.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset1.Content = lines[11];
                        preset1 = lines[12];
                    }
                    if (lines[15].Contains("N/A"))
                    {
                        btnPreset2.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset2.Content = lines[15];
                        preset2 = lines[16];

                    }
                    if (lines[19].Contains("N/A"))
                    {
                        btnPreset3.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset3.Content = lines[19];
                        preset3 = lines[20];
                    }
                    //if (lines[23].Contains("N/A"))
                    //{
                    //    btnPreset4.Visibility = Visibility.Collapsed;
                    //}
                    //else 
                    //{
                    //    btnPreset4.Content = lines[23];
                    //    preset4 = lines[24];
                    //}

                    lblPresetsHeader.Text = "Device Specific Power Presets:";
                }
                else
                {
                    double capacity = 0;
                    int speed = 0;
                    int type = 0;
                    try
                    {
                        ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PhysicalMemory");

                        foreach (ManagementObject queryObj in searcher.Get())
                        {
                            capacity = capacity + Convert.ToDouble(queryObj["Capacity"]);
                            speed = Convert.ToInt32(queryObj["Speed"]);
                            type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]);
                        }

                        

                        capacity = capacity / 1024 / 1024 / 1024;
                        

                        string DDRType = "";
                        if (type == 26) DDRType = "DDR4";
                        else if (type == 30) DDRType = "LPDDR4";
                        else if (type == 35) DDRType = "LPDDR5";
                        else DDRType = $"Unknown ({type})";

                        lblRAM.Text = $"RAM: {capacity}GB {DDRType} {speed}MT/s";
                        
                    }
                    catch (ManagementException e)
                    {
                        lblRAM.Text = "RAM: N/A";
                    }

                    double diskSize = 0;

                    try
                    {

                        ManagementObjectSearcher searcher =
                            new ManagementObjectSearcher("root\\CIMV2",
                            "SELECT * FROM Win32_DiskPartition");

                        foreach (ManagementObject queryObj in searcher.Get())
                        {
                            diskSize = diskSize + Convert.ToDouble(queryObj["Size"]);
                        }

                        diskSize = diskSize / 1024 / 1024 / 1024;
                        diskSize = Math.Round(diskSize);

                        if (diskSize > 1000)
                        {
                            diskSize = diskSize / 1024;
                            lblStore.Text = $"Storage: {Math.Round(diskSize, 2)} TB";
                        }
                        else lblStore.Text = $"Storage: {diskSize} GB";
                    }
                    catch (ManagementException e)
                    {
                        lblStore.Text = "Storage: N/A";
                    }

                    try
                    {
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            DEVMODE dm = new DEVMODE();
                            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                            EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
                            Console.WriteLine($"Device: {screen.DeviceName}");
                            if(screen.DeviceName.Contains("1")) lblDisplay.Text = $"Display: {dm.dmPelsWidth} x {dm.dmPelsHeight}";
                        } 
                    }
                    catch
                    {
                        lblDisplay.Text = "Display: N/A";
                    }

                    imgDevice.Source = new BitmapImage(new Uri(apuPreset + "\\config.png"));

                    lblPresetName.Text = "APU Preset: " + apuname;
                    lblCPU.Text = "CPU: " + apuname;
                    lblGPU.Text = "GPU: Radeon™ Graphics";

                    if (apuname.Contains("U"))
                    {
                        APUPresetToLoad = "U";
                    }
                    if (apuname.Contains("HX"))
                    {
                        APUPresetToLoad = "HX";
                    }
                    if (apuname.Contains("H") || apuname.Contains("Z"))
                    {
                        APUPresetToLoad = "H-HS";
                    }
                    if (apuname.Contains("GE"))
                    {
                        APUPresetToLoad = "GE";
                        imgDevice.Source = new BitmapImage(new Uri(apuPreset + "\\config-DT-AM4.png"));
                    }
                    else if (apuname.Contains("G"))
                    {
                        APUPresetToLoad = "G";
                        imgDevice.Source = new BitmapImage(new Uri(apuPreset + "\\config-DT-AM4.png"));
                    }

                    var lines = File.ReadAllLines(apuPreset + $"\\{APUPresetToLoad}\\config.txt");

                    if (lines[11].Contains("N/A"))
                    {
                        btnPreset1.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset1.Content = lines[11];
                        preset1 = lines[12];
                    }
                    if (lines[15].Contains("N/A"))
                    {
                        btnPreset2.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset2.Content = lines[15];
                        preset2 = lines[16];

                    }
                    if (lines[19].Contains("N/A"))
                    {
                        btnPreset3.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset3.Content = lines[19];
                        preset3 = lines[20];
                    }
                    //if (lines[23].Contains("N/A"))
                    //{
                    //    btnPreset4.Content = "Find Device Presets";
                    //}
                    //else 
                    //{
                    //    btnPreset4.Content = lines[23];
                    //    preset4 = lines[24];
                    //}

                    lblPresetsHeader.Text = "APU Specific Power Presets:";
                }


                if (Settings.Default.SnowPreset == 1) { btnPreset1.IsChecked = true; applyPreset1(); }
                else if (Settings.Default.SnowPreset == 2) { btnPreset2.IsChecked = true; applyPreset2(); }
                else if (Settings.Default.SnowPreset == 3) { btnPreset3.IsChecked = true; applyPreset3(); }

            } catch (Exception ex)
            {
                MessageBox.Show("An error occured loading presets. Error: " + ex.Message);
            }
        }

        private void btnPreset1_Click(object sender, RoutedEventArgs e)
        {
            applyPreset1();
        }

        public void applyPreset1()
        {
            if (preset1 == null || preset1 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                //BasicExeBackend.ApplySettings(pathRyzenAdj, preset1, true);
                RyzenAdj_To_UXTU.Translate(preset1);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset1;
                Settings.Default.SnowPreset = 1;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        private void btnPreset2_Click(object sender, RoutedEventArgs e)
        {
            applyPreset2();
        }

        public void applyPreset2()
        {
            if (preset2 == null || preset2 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                //BasicExeBackend.ApplySettings(pathRyzenAdj, preset2, true);
                RyzenAdj_To_UXTU.Translate(preset2);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset2;
                Settings.Default.SnowPreset = 2;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        private void btnPreset3_Click(object sender, RoutedEventArgs e)
        {
            applyPreset3();
        }

        public void applyPreset3()
        {
            if (preset3 == null || preset3 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                //string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                //BasicExeBackend.ApplySettings(pathRyzenAdj, preset3, true);
                RyzenAdj_To_UXTU.Translate(preset3);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default.SnowPreset = 3;
                Settings.Default["RyzenAdjArguments"] = preset3;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        //private void btnPreset4_Click(object sender, RoutedEventArgs e)
        //{
        //    if (preset4 == null || preset4 == "" && btnPreset4.Content != "Find Device Presets")
        //    {
        //        BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
        //    }
        //    if (btnPreset4.Content == "Find Device Presets")
        //    {
        //        System.Diagnostics.Process.Start("https://www.dropbox.com/sh/5yll57epokkcqgs/AABVsb_cjkdmMoss5JsMK4fia?dl=0");
        //    }

        //    else
        //    {
        //        //Get RyzenAdj path
        //        string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
        //        //Pass settings on to be applied
        //        BasicExeBackend.ApplySettings(pathRyzenAdj, preset4, true);
        //        BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
        //        Settings.Default["RyzenAdjArguments"] = preset4;
        //        Settings.Default.Save();
        //        //System.Windows.MessageBox.Show(ryzenadj);
        //    }
        //}
    }
}
