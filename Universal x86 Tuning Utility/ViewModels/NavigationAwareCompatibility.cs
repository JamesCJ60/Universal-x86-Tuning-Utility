using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class DataViewModel
{
    public Task OnNavigatedToAsync()
    {
        OnNavigatedTo();
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync()
    {
        OnNavigatedFrom();
        return Task.CompletedTask;
    }
}
