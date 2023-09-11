using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaBinding.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
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
    public Task<string> BindingToTask { get; set; }

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

}

/// <summary>
///     表示一个带索引的项。
/// </summary>
/// <param name="Index"> 索引 </param>
/// <param name="Text"> 文本 </param>
public record IndexedItem(int Index, string Text);
