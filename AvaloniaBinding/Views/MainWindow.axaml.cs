using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaBinding.Controls;

namespace AvaloniaBinding.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        BindingFromCode();
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Opacity = 1;
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Opacity = 0.5;
    }

    /// <summary>
    ///     通过代码绑定的示例。
    /// </summary>
    private void BindingFromCode()
    {
        OutputTextBox[!OneWayTextBox.OneWayTextProperty] = InputTextBox[!OneWayTextBox.OneWayTextProperty];

        // Avalonia 框架为 AvaloniaProperty 类型提供了 ! 运算符重载，用于创建 IndexerDescriptor 对象。
        // IndexerDescriptor 对象用于描述 AvaloniaObject 的 [] 绑定信息。
        // 参见：https://github.com/AvaloniaUI/Avalonia/blob/9eeb3ee3400e9717b92b1e740b50f9de0c222b12/src/Avalonia.Base/AvaloniaProperty.cs#L163-L176

        // 还重载了 [] 运算符，用于获取或设置 AvaloniaObject 的绑定信息。
        // 参见：https://github.com/AvaloniaUI/Avalonia/blob/9eeb3ee3400e9717b92b1e740b50f9de0c222b12/src/Avalonia.Base/AvaloniaObject.cs#L92-L100

        // 以上代码等效于：
        // IndexerDescriptor inputBindingDescriptor = new IndexerDescriptor
        // {
        //     Priority = BindingPriority.LocalValue,
        //     Property = OneWayTextBox.OneWayTextProperty,
        // };
        // IndexerBinding inputIndexerBinding = new IndexerBinding(
        //     this, inputBindingDescriptor.Property!, inputBindingDescriptor.Mode);
        // IndexerDescriptor outputBindingDescriptor = new IndexerDescriptor
        // {
        //     Priority = BindingPriority.LocalValue,
        //     Property = OneWayTextBox.OneWayTextProperty,
        // };
        // OutputTextBlock.Bind(outputBindingDescriptor.Property!, inputIndexerBinding);

        // 此外，AvaloniaProperty 类型还重载了 ~ 运算符，其所创建的绑定的优先级 Priority 为 BindingPriority.Template。
        // ~OneWayTextBox.OneWayTextProperty 等效于
        // IndexerDescriptor inputBindingDescriptor = new IndexerDescriptor
        // {
        //     Priority = BindingPriority.Template,
        //     Property = OneWayTextBox.OneWayTextProperty,
        // };

        TwoWayOutputTextBox[~!OneWayTextBox.OneWayTextProperty] = TwoWayInputTextBox[~!OneWayTextBox.OneWayTextProperty];

        // IndexerDescriptor 也重载了 ! 及 ~ 运算符，功能等效，用于设置 IndexerDescriptor 的绑定模式为 BindingMode.TwoWay。
        // 参见：https://github.com/AvaloniaUI/Avalonia/blob/9eeb3ee3400e9717b92b1e740b50f9de0c222b12/src/Avalonia.Base/Data/IndexerDescriptor.cs#L63-L81
        // !!OneWayTextBox.OneWayTextProperty 等效于 ~!OneWayTextBox.OneWayTextProperty
        // !~OneWayTextBox.OneWayTextProperty 等效于 ~~OneWayTextBox.OneWayTextProperty
    }

    static MainWindow()
    {
        Window.PointerExitedEvent.AddClassHandler<Window>((toolTip, args) =>
        {
            args.Handled = true;
        }, RoutingStrategies.Tunnel);
    }
}
