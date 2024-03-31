using Avalonia;
using Avalonia.Controls;
using AvaloniaPrism.Services;
using Prism.Ioc;

namespace AvaloniaPrism.Views;

public partial class DialogsView : UserControl
{
    public DialogsView()
    {
        InitializeComponent();
    }


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Initialize the WindowNotificationManager with the "TopLevel". Previously (v0.10), MainWindow
        INotificationService? notifyService = ContainerLocator.Current.Resolve<INotificationService>();
        notifyService.SetHostWindow(TopLevel.GetTopLevel(this)!);
    }
}

