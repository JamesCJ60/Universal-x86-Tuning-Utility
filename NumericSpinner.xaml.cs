using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stopbyte.Controls
{
    /// <summary>
    /// Interaction logic for NumericSpinner.xaml
    /// </summary>
    public partial class NumericSpinner : UserControl
    {
        #region Fields

        public event EventHandler PropertyChanged;
        public event EventHandler ValueChanged;
        #endregion

        public NumericSpinner()
        {
            try
            {
                InitializeComponent();

                tb_main.SetBinding(TextBox.TextProperty, new Binding("Value")
                {
                    ElementName = "root_numeric_spinner",
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

                DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(NumericSpinner)).AddValueChanged(this, PropertyChanged);
                DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(NumericSpinner)).AddValueChanged(this, ValueChanged);
                DependencyPropertyDescriptor.FromProperty(DecimalsProperty, typeof(NumericSpinner)).AddValueChanged(this, PropertyChanged);
                DependencyPropertyDescriptor.FromProperty(MinValueProperty, typeof(NumericSpinner)).AddValueChanged(this, PropertyChanged);
                DependencyPropertyDescriptor.FromProperty(MaxValueProperty, typeof(NumericSpinner)).AddValueChanged(this, PropertyChanged);

                PropertyChanged += (x, y) => validate();
            }
            catch { }
        }

        #region ValueProperty

        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(new decimal(0)));

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set
            {
                try
                {
                    if (value < MinValue)
                        value = MinValue;
                    if (value > MaxValue)
                        value = MaxValue;
                    SetValue(ValueProperty, value);
                    ValueChanged(this, new EventArgs());
                }
                catch { }
            }
        }


        #endregion

        #region StepProperty

        public readonly static DependencyProperty StepProperty = DependencyProperty.Register(
            "Step",
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(new decimal(0.1))); 

        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set
            {
                try
                {
                    SetValue(StepProperty, value);
                }
                catch { }
            }
        }

        #endregion

        #region DecimalsProperty

        public readonly static DependencyProperty DecimalsProperty = DependencyProperty.Register(
            "Decimals",
            typeof(int),
            typeof(NumericSpinner),
            new PropertyMetadata(2));

        public int Decimals
        {
            get { return (int)GetValue(DecimalsProperty); }
            set
            {
                try
                {
                    SetValue(DecimalsProperty, value);
                }
                catch { }
            }
        }

        #endregion

        #region MinValueProperty

        public readonly static DependencyProperty MinValueProperty = DependencyProperty.Register(
            "MinValue",
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(decimal.MinValue));

        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set
            {
                try
                {
                    if (value > MaxValue)
                        MaxValue = value;
                    SetValue(MinValueProperty, value);
                }
                catch { }
            }
        }

        #endregion

        #region MaxValueProperty

        public readonly static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue",
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(decimal.MaxValue));

        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set
            {
                try
                {
                    if (value < MinValue)
                        value = MinValue;
                    SetValue(MaxValueProperty, value);
                }
                catch { }
            }
        }

        #endregion

        /// <summary>
        /// Revalidate the object, whenever a value is changed...
        /// </summary>
        private void validate()
        {
            try
            {
                // Logically, This is not needed at all... as it's handled within other properties...
                if (MinValue > MaxValue) MinValue = MaxValue;
                if (MaxValue < MinValue) MaxValue = MinValue;
                if (Value < MinValue) Value = MinValue;
                if (Value > MaxValue) Value = MaxValue;
            }
            catch { }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Value += Step;
            }
            catch(Exception ex) { }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Value -= Step;
            }
            catch(Exception ex) { }
        }

        private void tb_main_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ValueChanged(this, new EventArgs());
            }
            catch (Exception ex) { }
        }
    }
}
