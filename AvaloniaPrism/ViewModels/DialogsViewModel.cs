using System.Windows.Input;
using AvaloniaPrism.Services;
using AvaloniaPrism.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace AvaloniaPrism.ViewModels;

public class DialogsViewModel : BindableBase
{
    private string _dialogContent = string.Empty;

    public DialogsViewModel(IDialogService dialogService, INotificationService notificationService)
    {
        ShowDialogCommand = new DelegateCommand(() =>
        {
            DialogParameters parameters = new()
            {
                { "title", "Dialog Title" },
                { "content", DialogContent }
            };

            dialogService.ShowDialog(nameof(MessageBoxView), parameters, result =>
            {
                DialogContent = result.Result == ButtonResult.OK
                    ? "Dialog closed by OK"
                    : "Dialog closed by Cancel";
            });
        });
        ShowNotificationCommand = new DelegateCommand(() =>
            notificationService.Show("Notification Title", "Notification Content"));
    }

    public string DialogContent
    {
        get => _dialogContent;
        set => SetProperty(ref _dialogContent, value);
    }

    public ICommand ShowDialogCommand { get; set; }

    public ICommand ShowNotificationCommand { get; set; }
}
