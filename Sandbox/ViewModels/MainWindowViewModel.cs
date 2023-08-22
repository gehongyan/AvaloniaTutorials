using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sandbox.Models;

namespace Sandbox.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    ///     性别
    /// </summary>
    [Reactive]
    public Gender? IndividualGender { get; set; }

    /// <summary>
    ///     设置性别的命令
    /// </summary>
    public ICommand SetValueIndividuallyCommand => ReactiveCommand.Create<Gender>(gender => IndividualGender = gender);

    /// <summary>
    ///     性别
    /// </summary>
    [Reactive]
    public Gender? UnifiedGender { get; set; }

    /// <summary>
    ///     设置性别的命令
    /// </summary>
    public ICommand SetValueUnifiedCommand => ReactiveCommand.Create<Gender>(gender => UnifiedGender = gender);
}
