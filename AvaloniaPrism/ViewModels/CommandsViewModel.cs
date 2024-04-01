using System;
using Prism;
using Prism.Commands;
using Prism.Mvvm;

namespace AvaloniaPrism.ViewModels;

public class CommandsViewModel : BindableBase, IActiveAware, IDisposable
{
    private int _currentNumber;
    private bool _userSetCanIncrement;
    private bool _userSetCanDecrement;
    private string _log = string.Empty;
    private bool _isActive;

    public CommandsViewModel()
    {
        IncrementCommand = new DelegateCommand(Increment, CanIncrement);
        DecrementCommand = new DelegateCommand(Decrement).ObservesCanExecute(() => UserSetCanDecrement);
        CompositeCommand = new CompositeCommand();
        CompositeCommand.RegisterCommand(IncrementCommand);
        CompositeCommand.RegisterCommand(DecrementCommand);
    }

    public string Log
    {
        get => _log;
        set => SetProperty(ref _log, value);
    }

    public int CurrentNumber
    {
        get => _currentNumber;
        set => SetProperty(ref _currentNumber, value);
    }

    #region DelegateCommand with RaiseCanExecuteChanged

    public bool UserSetCanIncrement
    {
        get => _userSetCanIncrement;
        set
        {
            SetProperty(ref _userSetCanIncrement, value);
            IncrementCommand.RaiseCanExecuteChanged();
        }
    }

    private void Increment()
    {
        CurrentNumber++;
        Log += "CurrentNumber incremented.\n";
    }

    private bool CanIncrement() => UserSetCanIncrement;

    public DelegateCommand IncrementCommand { get; private set; }

    #endregion

    #region DelegateCommand with ObservesCanExecute

    public bool UserSetCanDecrement
    {
        get => _userSetCanDecrement;
        set => SetProperty(ref _userSetCanDecrement, value);
    }

    private void Decrement()
    {
        CurrentNumber--;
        Log += "CurrentNumber decremented.\n";
    }

    private bool CanDecrement() => UserSetCanDecrement;

    public DelegateCommand DecrementCommand { get; private set; }

    #endregion

    #region CompositeCommand

    public CompositeCommand CompositeCommand { get; private set; }


    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        CompositeCommand.UnregisterCommand(IncrementCommand);
        CompositeCommand.UnregisterCommand(DecrementCommand);
    }

    /// <inheritdoc />
    public bool IsActive
    {
        get => _isActive;
        set
        {
            SetProperty(ref _isActive, value);
            OnIsActiveChanged();
        }
    }

    /// <inheritdoc />
    public event EventHandler? IsActiveChanged;

    private void OnIsActiveChanged()
    {
        IncrementCommand.IsActive = IsActive;
        DecrementCommand.IsActive = IsActive;
        IsActiveChanged?.Invoke(this, EventArgs.Empty);
        Log += $"IsActive changed to {IsActive}.\n";
    }

    #endregion

}
