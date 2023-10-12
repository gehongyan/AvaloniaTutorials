using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Sandbox.Behaviors;

public class DataGridSyncScrollBehavior : Behavior<DataGrid>
{
    /// <summary>
    ///     Defines the <see cref="SyncedDataGrid"/> property
    /// </summary>
    public static readonly StyledProperty<DataGrid?> SyncedDataGridProperty =
        AvaloniaProperty.Register<DataGridSyncScrollBehavior, DataGrid?>(nameof(SyncedDataGrid));

    /// <summary>
    ///     Gets or sets the scroll synced <see cref="DataGrid"/>
    /// </summary>
    [ResolveByName]
    public DataGrid? SyncedDataGrid
    {
        get => GetValue(SyncedDataGridProperty);
        set => SetValue(SyncedDataGridProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        if (SyncedDataGrid is null || AssociatedObject is null)
            return;
        SyncedDataGrid.TemplateApplied += BindingScroll;
        AssociatedObject.TemplateApplied += BindingScroll;
    }

    private void BindingScroll(object? sender, TemplateAppliedEventArgs e)
    {
        if (SyncedDataGrid?.GetTemplateChildren().OfType<ScrollBar>()
                .FirstOrDefault(x => x.Name == "PART_VerticalScrollbar") is not { } syncedScrollBar
            || AssociatedObject?.GetTemplateChildren().OfType<ScrollBar>()
                    .FirstOrDefault(x => x.Name == "PART_VerticalScrollbar") is not { } currentScrollBar)
            return;

        // 处理鼠标滚轮
        SyncedDataGrid.AddHandler(InputElement.PointerWheelChangedEvent, (o, args) =>
        {
            typeof(DataGrid)
                .GetMethod("OnPointerWheelChanged", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(AssociatedObject, new[] {args});
        }, RoutingStrategies.Bubble, true);
        AssociatedObject.AddHandler(InputElement.PointerWheelChangedEvent, (o, args) =>
        {
            typeof(DataGrid)
                .GetMethod("OnPointerWheelChanged", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(SyncedDataGrid, new[] {args});
        }, RoutingStrategies.Bubble, true);

        // 双向绑定滚动条值
        syncedScrollBar[~!RangeBase.ValueProperty] = currentScrollBar[~!RangeBase.ValueProperty];

        // 处理滚动事件
        syncedScrollBar.Scroll += (o, args) =>
        {
            typeof(DataGrid)
                .GetMethod("VerticalScrollBar_Scroll", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(AssociatedObject, new[] {o, args});
        };
        currentScrollBar.Scroll += (o, args) =>
        {
            typeof(DataGrid)
                .GetMethod("VerticalScrollBar_Scroll", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(SyncedDataGrid, new[] {o, args});
        };

    }
}
