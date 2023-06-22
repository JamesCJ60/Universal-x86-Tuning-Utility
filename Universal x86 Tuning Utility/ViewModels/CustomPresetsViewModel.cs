using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Universal_x86_Tuning_Utility.Models;
using Wpf.Ui.Common.Interfaces;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class CustomPresetsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {

        }

        private void InitializeViewModel()
        {
            
        }
    }
}
