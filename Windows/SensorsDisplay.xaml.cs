using AATUV3.Scripts.SMU_Backend_Scripts;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using RyzenSmu;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using UXTU.Properties;

namespace UXTU.Windows
{
    /// <summary>
    /// Interaction logic for SensorsDisplay.xaml
    /// </summary>
    /// 

    public class MyData : INotifyPropertyChanged
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }
        private string _SensorName;
        public string SensorName
        {
            get { return _SensorName; }
            set
            {
                _SensorName = value;
                OnPropertyChanged(nameof(SensorName));
            }
        }

        private uint _Address;
        public uint Address
        {
            get { return _Address; }
            set
            {
                _Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private float _value;
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class SensorsDisplay : Window
    {
        private DispatcherTimer _timer;
        public ObservableCollection<MyData> MyDataCollection { get; set; }
        public SensorsDisplay()
        {
            InitializeComponent();
            MyDataCollection = new ObservableCollection<MyData>();

            lblAPUName.Text = $"APU: {Settings.Default.APUName.Replace("AMD ", "")}with Radeon Graphics";
            lblSMU.Text = $"SMU Version: {Addresses.SMUVersion}";
            lblPMTable.Text = $"PMTable Version: 0x00{string.Format("{0:x}", Addresses.PMTableVersion)}";
            lblNumSensors.Text = $"Total Sensors: {pmtables.PMT_Offset.Length}";
            // add some test data to the collection
            for (int i = 0; i < pmtables.PMT_Offset.Length; i++)
            {
                string sensorName = pmtables.PMT_Sensors[i];
                if (Families.FAMID == 8) sensorName = "Uknown";

                MyDataCollection.Add(new MyData
                {
                    Index = i + 1,
                    SensorName = sensorName,
                    Address = pmtables.PMT_Offset[i],
                    Value = Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i])
            });
            }

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

                for (int i = 0; i < pmtables.PMT_Offset.Length; i++)
                {
                    MyDataCollection[i].Value = (float)Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]);
                }

                DataContext = this;

            if (_timer.Interval != TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value)))
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value));
                _timer.Start();
            }
        }

        private void nudSampleRate_ValueChanged(object sender, EventArgs e)
        {
            if (_timer.Interval != TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value)))
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value));
                _timer.Start();
            }
        }
    }
}
