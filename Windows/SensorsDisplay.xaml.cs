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

        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        private double _valueMin;
        public double ValueMin
        {
            get { return _valueMin; }
            set
            {
                _valueMin = value;
                OnPropertyChanged(nameof(ValueMin));
            }
        }

        private double _valueMax;
        public double ValueMax
        {
            get { return _valueMax; }
            set
            {
                _valueMax = value;
                OnPropertyChanged(nameof(ValueMax));
            }
        }

        private double _valueAvg;
        public double ValueAvg
        {
            get { return _valueAvg; }
            set
            {
                _valueAvg = value;
                OnPropertyChanged(nameof(ValueAvg));
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
        private int sensors = 0;
        public SensorsDisplay()
        {
            InitializeComponent();
            MyDataCollection = new ObservableCollection<MyData>();

            lblAPUName.Text = $"APU: {Settings.Default.APUName.Replace("AMD ", "")}with Radeon Graphics";
            lblSMU.Text = $"SMU Version: {Addresses.SMUVersion}";
            lblPMTable.Text = $"PMTable Version: 0x00{string.Format("{0:x}", Addresses.PMTableVersion).ToUpper()}";

            // add some test data to the collection
            for (int i = 0; i < pmtables.PMT_Offset.Length; i++)
            {
                if (i < pmtables.PMT_Sensors.Length)
                {
                    string sensorName = pmtables.PMT_Sensors[i];
                    if (Families.FAMID == 8 && i > 21) sensorName = "Unknown";

                    MyDataCollection.Add(new MyData
                    {
                        Index = i + 1,
                        SensorName = sensorName,
                        Address = pmtables.PMT_Offset[i],
                        Value = Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2),
                        ValueMin = Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2),
                        ValueMax = Math.Round( Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2),
                        ValueAvg = 0
                    });
                    lblNumSensors.Text = $"Total Sensors: {i + 1}";
                    sensors = i + 1;
                }
                else
                {
                    lblNumSensors.Text = $"Total Sensors: {i + 1}";
                    sensors = i + 1;
                }
            }

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.75);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private double[] avgValue;
        private int x = -1;
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (x == -1)
                {
                    avgValue = new double[sensors];
                    x = 1;
                }

                for (int i = 0; i < pmtables.PMT_Offset.Length; i++)
                {
                    if (i < pmtables.PMT_Sensors.Length)
                    {
                        MyDataCollection[i].Value = Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2);

                        if ((float)Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]) > MyDataCollection[i].ValueMax) MyDataCollection[i].ValueMax = Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2);
                        if ((float)Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]) < MyDataCollection[i].ValueMin) MyDataCollection[i].ValueMin = Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2);
                        if(x >= 1)
                        {
                            avgValue[i] = avgValue[i] + Math.Round(Smu.ReadFloat(Addresses.Address, pmtables.PMT_Offset[i]), 2);
                            MyDataCollection[i].ValueAvg = Math.Round(avgValue[i] / x, 2);
                        }
                    }
                }

                DataContext = this;

                x++;

                if (x > 10)
                {
                    x = 0;
                    avgValue = new double[sensors];
                }

                if (_timer.Interval != TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value)))
                {
                    _timer.Stop();
                    _timer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(nudSampleRate.Value));
                    _timer.Start();
                }
            }
            catch { }
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
