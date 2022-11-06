using System;
using System.Collections.Generic;
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
using RyzenSMUBackend;
using AATUV3.Scripts.SMU_Backend_Scripts;
using AATUV3.Scripts;
using UXTU.Scripts.Intel;
using UXTU.Properties;
using System.IO;

namespace AATUV3.Pages.AMDCPU
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class AMDCustomPresets : UserControl
    {
        public AMDCustomPresets()
        {
            InitializeComponent();
            if (File.Exists(Settings.Default.Path + "\\AC-LastUsedSettings.txt"))
            {
                var lines = File.ReadAllLines(Settings.Default.Path + "\\AC-LastUsedSettings.txt");

                bool temp, power, PBO;
                int tempLimit, pptLimit, tdcLimit, edcLimit, PBOScal;

                temp = Convert.ToBoolean(lines[1]);
                power = Convert.ToBoolean(lines[5]);
                PBO = Convert.ToBoolean(lines[11]);

                cbTemp.IsChecked = temp;
                cbPower.IsChecked = power;
                cbPBO.IsChecked = PBO;

                tempLimit = Convert.ToInt32(lines[2]);
                pptLimit = Convert.ToInt32(lines[6]);
                tdcLimit = Convert.ToInt32(lines[7]);
                edcLimit = Convert.ToInt32(lines[8]);
                PBOScal = Convert.ToInt32(lines[12]);

                nudTJ.Value = tempLimit;
                nudPPT.Value = pptLimit;
                nudTDC.Value = tdcLimit;
                nudEDC.Value = edcLimit;
                nudpboScaler.Value = PBOScal;

            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint TJ = (uint)nudTJ.Value;
                uint PPT = (uint)nudPPT.Value * 1000;
                uint EDC = (uint)nudEDC.Value * 1000;
                uint TDC = (uint)nudTDC.Value * 1000;
                uint Scaler = (uint)nudpboScaler.Value;

                if (cbTemp.IsChecked == true)
                {
                    SendCommand.set_tctl_temp(TJ);
                }

                if (cbPower.IsChecked == true)
                {
                    SendCommand.set_ppt(PPT);
                    SendCommand.set_edc(EDC);
                    SendCommand.set_tdc(TDC);
                }


                if (cbPBO.IsChecked == true)
                {
                    SendCommand.set_scaler(Scaler);
                }

                string LastUsed = $"[TJMax Limit]\n{cbTemp.IsChecked.ToString()}\n{nudTJ.Value.ToString()}\n\n[Power Limits]\n{cbPower.IsChecked.ToString()}\n{nudPPT.Value.ToString()}\n{nudTDC.Value.ToString()}\n{nudEDC.Value.ToString()}\n\n[PBO]\n{cbPBO.IsChecked.ToString()}\n{nudpboScaler.Value.ToString()}";
                File.WriteAllText(Settings.Default.Path + "\\AC-LastUsedSettings.txt", LastUsed);

                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
            }
            catch (Exception ex)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-was-an-issue-applying-settings!", false);
            }
        }
    }
}
