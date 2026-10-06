using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Universal_x86_Tuning_Utility.Models;

public partial class SystemInformationState : ObservableObject
{
    [ObservableProperty]
    private string _deviceNameText = "";

    [ObservableProperty]
    private string _deviceProducerText = "";

    [ObservableProperty]
    private string _deviceModelText = "";

    [ObservableProperty]
    private Visibility _cPUVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility _codeVisibility = Visibility.Visible;

    [ObservableProperty]
    private string _processorText = "";

    [ObservableProperty]
    private string _producerText = "";

    [ObservableProperty]
    private string _codenameText = "";

    [ObservableProperty]
    private Visibility _codenameVisibility = Visibility.Visible;

    [ObservableProperty]
    private string _captionText = "";

    [ObservableProperty]
    private string _coresText = "";

    [ObservableProperty]
    private string _threadsText = "";

    [ObservableProperty]
    private string _baseClockText = "";

    [ObservableProperty]
    private string _l1CacheText = "";

    [ObservableProperty]
    private string _l2CacheText = "";

    [ObservableProperty]
    private string _l3CacheText = "";

    [ObservableProperty]
    private string _instructionsText = "";

    [ObservableProperty]
    private Thickness _rAMMargin = new(0, 9, 15, 0);

    [ObservableProperty]
    private Visibility _rAMVisibility = Visibility.Visible;

    [ObservableProperty]
    private string _rAMText = "";

    [ObservableProperty]
    private string _rAMProducerText = "";

    [ObservableProperty]
    private string _rAMModelText = "";

    [ObservableProperty]
    private string _widthText = "";

    [ObservableProperty]
    private string _slotsText = "";

    [ObservableProperty]
    private Visibility _rAMTimeVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility _batteryVisibility = Visibility.Visible;

    [ObservableProperty]
    private string _healthText = "";

    [ObservableProperty]
    private string _cycleText = "";

    [ObservableProperty]
    private string _capcityText = "";

    [ObservableProperty]
    private string _chargeRateText = "";


}
