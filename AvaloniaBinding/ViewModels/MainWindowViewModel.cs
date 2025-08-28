using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Vulkan;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaBinding.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        TryGetActiveWin32CompositionMode(out Win32CompositionMode? mode);
        TryGetActiveWin32RenderingMode(out Win32ActiveRenderingMode? renderingMode);
    }

    #region Conventional

    /// <summary>
    ///     获取用于展示常规绑定方式的文本属性。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     一般地，ViewModel 属性要想与视图控件属性进行绑定，需要视图控件的属性需要有对应声明的
    ///     AvaloniaProperty 属性，例如本属性能绑定到 <see cref="TextBlock"/> 的 <see cref="TextBlock.Text"/>
    ///     是因为 <see cref="TextBlock"/> 有对应的只读静态字段 <see cref="TextBlock.TextProperty"/>
    ///     作为 AvaloniaProperty 属性的声明，属性的 <c>get</c> 与 <c>set</c> 方法会分别调用
    ///     <see cref="AvaloniaObject.GetValue{T}(StyledProperty{T})"/> 与
    ///     <see cref="AvaloniaObject.SetValue{T}(StyledProperty{T},T,BindingPriority)"/>
    ///     方法操作属性值。
    /// </para>
    /// <para>
    ///     <code lang="csharp">
    ///         // TextBlock.cs
    ///         public static readonly StyledProperty&lt;string?&gt; TextProperty =
    ///             AvaloniaProperty.Register&lt;TextBlock, string?&gt;(nameof(Text));
    ///         public string? Text
    ///         {
    ///             get =&gt; GetValue(TextProperty);
    ///             set =&gt; SetValue(TextProperty, value);
    ///         }
    ///     </code>
    /// </para>
    /// <para>
    ///     如果控件属性没有关联到 AvaloniaProperty 属性，那么就无法与 ViewModel
    ///     属性进行直接绑定，例如 <see cref="ComboBox"/> 的 <see cref="ComboBox.Items"/>
    ///     没有对应的 AvaloniaProperty，无法绑定。
    /// </para>
    /// <para>
    ///     在 ViewModel 侧，要绑定的属性的可访问性必须对控件所在视图可见，例如本属性的 <c>get</c> 访问性是
    ///     <c>internal</c>，即只有当前程序集中的文件中可以访问。如果将 ViewModel 定义在单独的程序集中，那么
    ///     <c>get</c> 访问性必须是 <c>public</c>，或通过 <see cref="InternalsVisibleToAttribute"/>
    ///     将 ViewModel 所在程序集的 <c>internal</c> 访问性公开给控件所在视图所在的程序集。
    ///     本属性值在本示例场景中不会变更，也不接收来自 View 的值，因此未提供 <c>set</c> 访问，声明为计算属性。
    /// </para>
    /// </remarks>
    internal string ConventionalBindingText => "Welcome to Avalonia!";

    #endregion

    #region Binding to Task or IObservable

    /// <summary>
    ///     获取用于展示绑定到 <see cref="Task{T}"/> 的文本属性。
    /// </summary>
    /// <remarks>
    ///     <see cref="Task{T}"/> 类型的属性可以直接进行绑定，在绑定表达式该属性名称后缀附加脱字符
    ///     <c>^</c>，编译器即可在接收到该 <see cref="Task{T}"/> 属性变更后等待其完成并获取其结果。
    /// </remarks>
    [Reactive]
    public Task<string> BindingToTask { get; set; } = Task.FromResult(string.Empty);

    /// <summary>
    ///     获取用于重新设置 <see cref="BindingToTask"/> 属性的命令。
    /// </summary>
    public ICommand RestartTaskCommand => ReactiveCommand.Create(() =>
        BindingToTask = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return $"It's {DateTime.Now:HH:mm:ss} now.";
        }));

    /// <summary>
    ///     获取用于展示绑定到 <see cref="IObservable{T}"/> 的文本属性。
    /// </summary>
    /// <remarks>
    ///     绑定到 <see cref="IObservable{T}"/> 的属性，绑定表达式的属性名称后缀附加脱字符
    ///     <c>^</c>，编译器即可在接收到该 <see cref="IObservable{T}"/> 可观察对象的值变更后获取其最新值。
    /// </remarks>
    public IObservable<string> BindingToObservable => Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(x => x.ToString());

    #endregion

    #region Compiled Binding

    /// <summary>
    ///     获取用于展示编译绑定方式的文本属性。
    /// </summary>
    /// <remarks>
    ///     Avalonia 中的编译绑定（Compiled Binding）是一种在编译时就确定绑定路径的绑定方式，相比于常规绑定，编译绑定
    ///     相比于运行时反射绑定，具有更高的性能和可靠性。使用编译绑定有以下几种方式：
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 使用 <see cref="CompiledBindingExtension"/>，在 XAML 中具体的绑定声明中指定编译绑定。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 使用 <c>x:CompileBindings="True"</c> 指定 XAML 节点及其子节点的 <c>Binding</c>
    ///                 应解析为 <see cref="CompiledBindingExtension"/>。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 在 <c>csproj</c> 项目文件的 <c>Project.PropertyGroup</c> 节点下新增节点
    ///                 <c>&lt;AvaloniaUseCompiledBindingsByDefault&gt;true&lt;/AvaloniaUseCompiledBindingsByDefault&gt;</c>。
    ///             </description>
    ///         </item>
    ///     </list>
    ///     编译绑定时，如果编译器无法推断类型，则需要通过 <c>x:DataType="YourDataType"</c> 或
    ///     <see cref="CompiledBindingExtension.DataType"/> 显式指定数据类型，这通常发生在以下几种情况下：
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 视图层文件根节点指定视图 <see cref="StyledElement.DataContext"/> 的类型。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 在样式或资源中传入绑定表达式。
    ///             </description>
    ///         </item>
    ///     </list>
    ///     如果编译绑定表达式中的某个属性访问无法推断类型，则可以使用强制类型转换表达式。例如：
    ///     <code lang="xml">
    ///         Text="{Binding $parent[Window].DataContext.Prefix}"
    ///     </code>
    ///     <c>DataContext</c> 属性的类型为 <c>object</c>，编译器无法解析 <c>object</c> 类型上的字段或属性
    ///     <c>Prefix</c>，因此需要进行强制类型转换，将 <c>DataContext</c> 强制转换为 <see cref="MainWindowViewModel"/> 类型。
    ///     <code lang="xml">
    ///         Text="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).Prefix}"
    ///     </code>
    ///     如需访问内部类，可以使用 <c>+</c> 连接符。例如，要访问 <see cref="MainWindowViewModel"/> 的
    ///     <c>InnerClass</c> 类，请指定 <c>MainWindowViewModel+InnerClass</c>。
    ///     如需在编译绑定上下文中禁用编译绑定，可以设置 <c>x:CompiledBinding="False"</c>，可以禁用该节点及其子节点的编译绑定。
    ///     也可以使用 <see cref="ReflectionBindingExtension"/> 来显式指定绑定表达式为反射绑定，这将只对绑定表达式有效。
    /// </remarks>
    public string CompiledBindingText => "Text in CompiledBinding demo.";

    #endregion

    #region Binding to Controls

    /// <summary>
    ///     获取用于展示绑定到控件的集合属性。
    /// </summary>
    /// <remarks>
    ///     本示例是将 <see cref="IEnumerable{T}"/> 类型的数据源绑定到 <see cref="ItemsControl"/>
    ///     并自定义 <see cref="ItemsControl.ItemTemplate"/> 的示例，示例中的成员模板内创建了
    ///     <see cref="Grid"/> 容器，包装了四个 <see cref="TextBlock"/>，分别绑定到了外部
    ///     祖先控件上的属性、当前数据成员的属性、命名控件的属性、自身控件的属性。
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 绑定到自身，可使用 <c>$self</c>。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 绑定到外部祖先控件，可使用 <c>$parent</c>。如需指定类型，可使用 <c>$parent[Type]</c>。
    ///                 例如，绑定到最近的祖先 <see cref="Window"/>，可使用 <c>$parent[Window]</c>。如需指定层级，
    ///                 可使用 <c>$parent[level]</c>，0 基索引，例如，要绑定到第三级祖先控件，可使用 <c>$parent[2]</c>。
    ///                 如需同时指定类型与层级，可使用 <c>$parent[Type;level]</c>，例如，要绑定到第二近的祖先
    ///                 <see cref="ItemsControl"/>，可使用 <c>$parent[ItemsControl;1]</c>。需要注意，在启用编译绑定时，
    ///                 仅指定层级不指定类型，可能会导致绑定失败。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 绑定到命名控件，可使用 <c>#name</c>。
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public IEnumerable<KeyValuePair<int, string>> ParentBindingItemsSource => Enumerable
        .Range(1, 5)
        .ToImmutableDictionary(x => x, x => $"Item {x}");

    /// <summary>
    ///     获取用于展示如何绑定到命名控件属性的文本属性。
    /// </summary>
    [Reactive]
    public string BindingToControlsText { get; set; } = "Suffix";

    /// <summary>
    ///     获取用于展示如何查找视图逻辑树上的祖先控件并绑定到其属性上的文本属性。
    /// </summary>
    public string Prefix => "Prefix";

    #endregion

    #region Template Binding

    /// <summary>
    ///     获取用于展示模板绑定方式的文本属性。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Avalonia 中的模板控件（Template Control）是一种无样式控件类，框架需要为模板控件提供默认样式，或由开发者在使用时指定样式。
    ///     模板可以理解为模板控件类中的各个属性值在视图中如何排版、展示、交互的规则。
    /// </para>
    /// <para>
    ///     本示例对 <see cref="Label"/> 的控件模板进行了重写，重新分配属性到不同的模板内部件上，这些部件上的属性值与
    ///     <see cref="Label"/> 控件的属性相关联，则需要使用 <see cref="TemplateBinding"/>。
    /// </para>
    /// </remarks>
    public string TemplateBindingText => "Text in TemplateBinding demo.";

    #endregion

    #region Assign Binding

    /// <summary>
    ///     获取用于展示分配绑定对象的控件数据源。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     也有一些控件属性的绑定不是为了与 ViewModel 的属性传递数据，而是为了允许开发者指定其它属性的绑定路径，例如
    ///     <see cref="ComboBox"/> 的 <see cref="SelectingItemsControl.SelectedValueBinding"/>，用于指定
    ///     <see cref="SelectingItemsControl.SelectedValue"/> 上标注了 <see cref="AssignBindingAttribute"/>
    ///     特性，标注 <see cref="InheritDataTypeFromItemsAttribute"/> 特性表示推断其 <see cref="IEnumerable{T}"/>
    ///     的泛型类型的属性。
    /// </para>
    /// <para>
    ///     <code lang="csharp">
    ///         // SelectingItemsControl.cs
    ///         [AssignBinding]
    ///         [InheritDataTypeFromItems(nameof(ItemsSource))]
    ///         public IBinding? SelectedValueBinding
    ///         {
    ///             get =&gt; GetValue(SelectedValueBindingProperty);
    ///             set =&gt; SetValue(SelectedValueBindingProperty, value);
    ///         }
    ///     </code>
    /// </para>
    /// <para>
    ///     <see cref="SelectingItemsControl.SelectedValueBinding"/> 的集合泛型推断来源为
    ///     <see cref="SelectingItemsControl.ItemsSource"/>，本示例中，<see cref="ImmutableDictionary{TKey,TValue}"/>
    ///     实现了 <see cref="IEnumerable{T}"/> 接口，其泛型类型为 <see cref="KeyValuePair{TKey,TValue}"/>，因此
    ///     <see cref="SelectingItemsControl.SelectedValueBinding"/> 的推断根类型为
    ///     <see cref="KeyValuePair{TKey,TValue}"/>。
    /// </para>
    /// <para>
    ///     本示例中，<see cref="ComboBox"/> 的 <see cref="SelectingItemsControl.SelectedValue"/> 的取值来源为
    ///     <see cref="KeyValuePair{TKey,TValue}"/> 的 <see cref="KeyValuePair{TKey,TValue}.Key"/> 属性。
    /// </para>
    /// </remarks>
    public IEnumerable<IndexedItem> AssignBindingItemsSource => Enumerable
        .Range(1, 5)
        .Select(x => new IndexedItem(x, $"Item {x}"));

    #endregion

    #region StringFormat

    /// <summary>
    ///     获取用于展示字符串格式化的文本属性。
    /// </summary>
    /// <remarks>
    ///     字符串格式化表达式中，可以使用 <c>{}</c> 作为占位符，表示绑定表达式中 0 基下标的某个绑定的值。
    ///     在单一表达式中，<c>{0}</c> 即为表示该绑定值。在 <see cref="MultiBinding"/> 表达式中，
    ///     <c>{0}</c> 表示第一个绑定值，<c>{1}</c> 表示第二个绑定值，以此类推。
    ///     占位符的格式化字符串与 C# 内插字符串的格式化字符串相同，例如，<c>{0:HH:mm:ss}</c> 表示
    ///     第一个绑定值的 <see cref="DateTime"/> 值以小时、分钟、秒格式化字符串。
    ///     如果字符串格式化表达式以左花括号 <c>{</c> 开头，那么则需要在表达式开头使用空下标访问器
    ///     <c>{}</c> 转义整个表达式，例如 <c>{}{0:HH:mm:ss}</c>。
    /// </remarks>
    public IEnumerable<IndexedItem> StringFormatItemsSource => AssignBindingItemsSource;

    #endregion

private static IAvaloniaDependencyResolver? AvaloniaLocator { get; } =
    typeof(AvaloniaLocator).GetProperty("Current")?.GetValue(null) as IAvaloniaDependencyResolver;

private static T? GetAvaloniaLocatorService<T>()
    where T : class
{
    if (AvaloniaLocator is not { } locator) return null;
    if (typeof(IAvaloniaDependencyResolver).GetMethod("GetService") is not { } method)
        return null;
    object? result = method.Invoke(locator, [typeof(T)]);
    return result as T;
}

public static bool TryGetActiveWin32CompositionMode([NotNullWhen(true)] out Win32CompositionMode? win32CompositionMode)
{
    if (!TryGetActiveWin32RenderingMode(out Win32ActiveRenderingMode? renderingMode)
        || renderingMode is not Win32ActiveRenderingMode.AngleEglD3D11)
    {
        win32CompositionMode = Win32CompositionMode.RedirectionSurface;
        return true;
    }
    IRenderTimer? renderTimer = GetAvaloniaLocatorService<IRenderTimer>();
    string? renderTimerClassName = renderTimer?.GetType().Name;
    win32CompositionMode = renderTimerClassName switch
    {
        "WinUiCompositorConnection" => Win32CompositionMode.WinUIComposition,
        "DirectCompositionConnection" => Win32CompositionMode.DirectComposition,
        "DxgiConnection" => Win32CompositionMode.LowLatencyDxgiSwapChain,
        _ => Win32CompositionMode.RedirectionSurface
    };
    return win32CompositionMode != null;
}

public static bool TryGetActiveWin32RenderingMode([NotNullWhen(true)] out Win32ActiveRenderingMode? win32RenderingMode)
{
    IPlatformGraphics? platformGraphics = GetAvaloniaLocatorService<IPlatformGraphics>();
    string? platformGraphicsClassName = platformGraphics?.GetType().Name;
    win32RenderingMode = platformGraphicsClassName switch
    {
        null when GetAvaloniaLocatorService<Win32PlatformOptions>()?.CustomPlatformGraphics is not null => Win32ActiveRenderingMode.Custom,
        null => Win32ActiveRenderingMode.Software,
        "D3D9AngleWin32PlatformGraphics" => Win32ActiveRenderingMode.AngleEglD3D9,
        "D3D11AngleWin32PlatformGraphics" => Win32ActiveRenderingMode.AngleEglD3D11,
        "WglPlatformOpenGlInterface" => Win32ActiveRenderingMode.Wgl,
        nameof(VulkanPlatformGraphics) => Win32ActiveRenderingMode.Vulkan,
        _ => null
    };

    return win32RenderingMode != null;
}

public enum Win32ActiveRenderingMode
{
    Custom,
    Software,
    AngleEglD3D9,
    AngleEglD3D11,
    Wgl,
    Vulkan
}
}

/// <summary>
///     表示一个带索引的项。
/// </summary>
/// <param name="Index"> 索引 </param>
/// <param name="Text"> 文本 </param>
public record IndexedItem(int Index, string Text);
