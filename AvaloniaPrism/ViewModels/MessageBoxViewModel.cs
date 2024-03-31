using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace AvaloniaPrism.ViewModels;

public class MessageBoxViewModel : BindableBase, IDialogAware
{
    private string _title = string.Empty;
    private string _content = string.Empty;
    private ICommand _cancelCommand;
    private ICommand _confirmCommand;

    public MessageBoxViewModel()
    {
        CancelCommand = new DelegateCommand(() => RaiseRequestClose(new DialogResult(ButtonResult.Cancel)));
        ConfirmCommand = new DelegateCommand(() => RaiseRequestClose(new DialogResult(ButtonResult.OK)));
    }

    /// <inheritdoc />
    public bool CanCloseDialog() => true;

    /// <inheritdoc />
    public void OnDialogClosed()
    {
    }

    /// <inheritdoc />
    public void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>("title");
        Content = parameters.GetValue<string>("content");
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <inheritdoc />
    public event Action<IDialogResult>? RequestClose;

    public virtual void RaiseRequestClose(IDialogResult dialogResult)
    {
        RequestClose?.Invoke(dialogResult);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public ICommand CancelCommand
    {
        get => _cancelCommand;
        set => SetProperty(ref _cancelCommand, value);
    }

    public ICommand ConfirmCommand
    {
        get => _confirmCommand;
        set => SetProperty(ref _confirmCommand, value);
    }
}
