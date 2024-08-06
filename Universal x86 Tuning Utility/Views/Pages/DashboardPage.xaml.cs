using System.Windows.Input;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Wpf.Ui.Common.Interfaces;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {
        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            _ = Tablet.TabletDevices;

            Garbage.Garbage_Collect();

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                caPremade.IsEnabled = false;
                btnPremade.IsEnabled = false;
            }
        }
    }
}