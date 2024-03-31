using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using AvaloniaPrism.ModuleA.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace AvaloniaPrism.ModuleA.ViewModels;

public class ModuleAViewModel : BindableBase
{
    private string _message = "Hello! This is Module a!";

    public ModuleAViewModel(IDelayService delayService)
    {
        DelayCommand = new DelegateCommand(() =>
            _ = Task.Run(async () =>
            {
                Dispatcher.UIThread.Post(() => Message = "Delay started");
                await delayService.DelayAsync(1000);
                Dispatcher.UIThread.Post(() => Message = "Delay completed");
            }));
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand DelayCommand { get; }
}
