using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universal_x86_Tuning_Utility.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Universal_x86_Tuning_Utility.Views.Pages;

public partial class Automations : INavigableView<AutomationsViewModel>
{
    public AutomationsViewModel ViewModel { get; }

    public Automations(AutomationsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await ViewModel.ActivateAsync();
        Unloaded += (_, _) => ViewModel.Deactivate();
    }

}
