using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace AvaloniaPrism.ViewModels;

public class NavigationsViewModel : BindableBase, INavigationAware
{
    private IRegionNavigationService? _navigationService;

    private string _message = string.Empty;

    public NavigationsViewModel()
    {
        GoBackCommand = new DelegateCommand<object>(GoBack, CanGoBack);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        DateTime at = navigationContext.Parameters.GetValue<DateTime>("at");
        Message += $"OnNavigatedTo invoked at {at}\n";
        _navigationService = navigationContext.NavigationService;
    }

    /// <inheritdoc />
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    /// <inheritdoc />
    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        Message += $"OnNavigatedFrom invoked at {DateTime.Now}\n";
    }

    public DelegateCommand<object> GoBackCommand { get; private set; }

    private void GoBack(object commandArg)
    {
        if (_navigationService?.Journal.CanGoBack is true)
        {
            _navigationService.Journal.GoBack();
        }
    }

    private bool CanGoBack(object commandArg) =>
        _navigationService?.Journal.CanGoBack ?? false;
}
