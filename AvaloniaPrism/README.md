# Avalonia 中的 Prism

> 源码 https://github.com/gehongyan/AvaloniaTutorials/tree/main/AvaloniaPrism

## 添加对 Prism 的引用

1. 安装 Prism 库

    ```bash
    dotnet add package Prism.Avalonia
    dotnet add package Prism.DryIoc.Avalonia
    ```

2. 在 `App.xaml.cs` 中添加 Prism 的引用

    ```csharp
    using Prism.Ioc;
    using Prism.DryIoc;
    ```

3. 在 `App.xaml.cs` 中修改基类为 `PrismApplication`

    ```csharp
    public partial class App : PrismApplication
    ```

4. 实现抽象方法 `RegisterTypes`

    ```csharp
    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }

    /// <inheritdoc />
    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }
    ```

5. 删除 `OnFrameworkInitializationCompleted`

6. 在 `Initialize` 中初始化 Prism

    ```csharp
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize(); // <-- 添加此行
    }
    ```
## 视图与视图模型的绑定

添加视图 `MainView` 及其视图模型 `MainViewModel`，在 `MainView` 中添加
`prism:ViewModelLocator.AutoWireViewModel="True"`，最后在 `MainWindow` 中引用该视图。
运行引用程序，可以发现，`MainView.DataContext` 被自动填充了 `MainViewModel` 的实例。
这是因为 Prism 会对设置了 `AutoWireViewModel` 为 `True` 的视图根据约定自动发现并绑定视图与视图模型。

不妨先看一下 Prism 绑定视图模型的源码：
> https://github.com/PrismLibrary/Prism/blob/2a25770c0afc99839c56c0d416b877a3788bd371/src/Prism.Core/Mvvm/ViewModelLocationProvider.cs#L114-L140

它首先查看是否为该视图注册了映射，如果没有，则会回退到基于约定的方法。

1. `GetViewModelForView`

该方法尝试从 `Dictionary<string, Func<object>> _factories` 中寻找键为指定视图类型名称的键值对，其值为对应的**视图模型的生成委托**。
这是最短完成解析的路径，因为这需要我们手动注册视图与视图模型生成委托的映射关系。

`Register` 的其中一个重载中访问了 `_factories` 索引器的 `set` 方法，将视图类型名称与视图模型生成委托的键值对添加到 `_factories` 中。
因此，我们可以在 `RegisterTypes` 中手动指定视图与视图模型生成委托的绑定关系。
```csharp
ViewModelLocationProvider.Register(typeof(MainView).FullName, () => new MainViewModel());
```

2. `GetViewModelTypeForView`

该方法尝试从 `Dictionary<string, Type> _typeFactories` 中寻找键为指定视图类型名称的键值对，其值为对应的**视图模型类型**。

`Register` 的另一个重载中访问了 `_typeFactories` 索引器的 `set` 方法，将视图类型名称与视图模型类型的键值对添加到 `_typeFactories` 中。
因此，我们可以在 `RegisterTypes` 中手动指定视图与视图模型类型的绑定关系。也存在另一个泛型重载，可以直接指定视图与视图模型类型的绑定关系。
```csharp
ViewModelLocationProvider.Register<MainView, MainViewModel>();
ViewModelLocationProvider.Register(typeof(MainView).FullName, typeof(MainViewModel));
```
3. 尝试从 `Func<object, Type> _defaultViewToViewModelTypeResolver` 生成视图模型类型。

这是由各个平台各自实现的默认视图模型类型生成委托，目前仅有 MAUI 设置了该解析器。

4. 尝试从 `Func<Type, Type> _defaultViewTypeToViewModelTypeResolver` 生成视图模型类型。

这是最后的回退方案，它会根据视图类型名称生成视图模型类型名称，然后尝试从程序集中加载该视图模型类型。
该生成委托的默认逻辑为：

```csharp
string viewName = viewType.FullName;
viewName = viewName.Replace(".Views.", ".ViewModels.");
string viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
string suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
string viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
return Type.GetType(viewModelName);
}
```

以 `viewType` 为 `typepf(MainView)` 为例：
- `viewName` 为 `AvaloniaPrism.Views.MainView`
- `viewName` 中的 `.Views.` 被替换为 `.ViewModels.`，得到 `AvaloniaPrism.ViewModels.MainView`
- `viewAssemblyName` 为 `AvaloniaPrism, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null`
- 如果 `viewName` 以 `View` 结尾，则 `suffix` 为 `Model`，否则为 `ViewModel`
- `viewModelName` 为 `AvaloniaPrism.ViewModels.MainViewModel, AvaloniaPrism, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null`
- 通过 `Type.GetType(viewModelName)` 获取到 `MainViewModel` 的类型

可以发现，要想实现自动转换，视图与视图模型之间需要遵循一定的约定：
- 视图模型位于与视图类型相同的程序集中
- 视图模型在一个名为 .ViewModels 的子命名空间中
- 视图位于一个名为 .Views 的子命名空间中
- 视图模型名称与视图名称相对应，并以 "ViewModel" 结尾

5. 如果获取到了视图模型的类型，则通过 `Activator.CreateInstance` 创建视图模型的实例，并通过`setDataContextCallback`
   将其赋值给视图的 `DataContext` 属性。

## 依赖注入

Prism 中的依赖注入主要是由 DryIoc 完成的，Microsoft.Extensions.DependencyInjection 则是在付费商用许可中提供。
因此，此处仅介绍 Avalonia 中 DryIoc 的使用。

### 生命周期

Prism 与大多数依赖注入容器一样，支持以下三种生命周期：
- Transient：瞬态，每次请求都会创建一个新的实例
- Singleton：单例，整个应用程序中只有一个实例
- Scoped：作用域，在同一作用域中只有一个实例

- 单例
```csharp
// 瞬态
containerRegistry.Register<IFooService, FooService>();
// 单例
containerRegistry.RegisterSingleton<IFooService, FooService>();
// 作用域
containerRegistry.RegisterScoped<IFooService, FooService>();
// 通过实例注册
containerRegistry.RegisterInstance<IFooService>(new FooService());
```

### 惰性解析

默认情况下，注册服务也会同步注册其相关的 `Func<IService>` 服务委托及 `Lazy<IService>` 惰性实例。
可以进一步将服务实例的解析推迟到第一次访问时，而不是在注册时立即解析。
但需要注意的是，如果服务是单例的，那惰性解析不一定会带来性能提升，因为单例服务在第一次解析时就会被创建，相反，惰性解析可能会带来额外的开销。

## 绑定

Prism 中提供了 `BindableBase` 类，它实现了 `INotifyPropertyChanged` 接口，可以方便地实现属性绑定。

```csharp
public class MainViewModel : BindableBase
{
    private string _title = "Hello World";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
```

## 命令

Prism 中的命令主要是通过 `DelegateCommand` 和 `CompositeCommand` 实现的。

### DelegateCommand

`DelegateCommand` 是一个泛型类，它接受一个泛型参数，表示命令的参数类型，也可以不传入参数类型，即为无参命令。

```csharp
public class MainViewModel : BindableBase
{
    public DelegateCommand SayHelloCommand { get; }

    public MainViewModel()
    {
        SayHelloCommand = new DelegateCommand(SayHello);
    }

    private void SayHello()
    {
        Title = "Hello Avalonia!";
    }
}
```

如果需要控制命令是否可用，可以通过 `CanExecute` 方法返回一个布尔值来实现。

```csharp
public class MainViewModel : BindableBase
{
    private string _title = "Hello World";

    public MainViewModel()
    {
        SayHelloCommand = new DelegateCommand(SayHello, CanSayHello); // <-- 修改此行
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public DelegateCommand SayHelloCommand { get; }

    private void SayHello()
    {
        Title = "Hello Avalonia!";
    }

    private bool CanSayHello() // <-- 添加此方法
    {
        return !string.IsNullOrEmpty(Title);
    }
}
```

如果 `CanExecute` 依赖其它可变值，可以通过 `RaiseCanExecuteChanged` 方法来主动通知命令重新计算是否可用。

```csharp
public class MainViewModel : BindableBase
{
    private string _title = "Hello World";

    public MainViewModel()
    {
        SayHelloCommand = new DelegateCommand(SayHello, CanSayHello);
    }

    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            SayHelloCommand.RaiseCanExecuteChanged(); // <-- 添加此行
        }
    }

    public DelegateCommand SayHelloCommand { get; }

    private void SayHello()
    {
        Title = "Hello Avalonia!";
    }

    private bool CanSayHello()
    {
        return !string.IsNullOrEmpty(Title);
    }
}
```

也可以通过 `ObservesCanExecute` 来指定命令是否可执行所依赖的属性变更。
```csharp
public class MainViewModel : BindableBase
{
    private bool _canSayHello = true;

    public MainViewModel()
    {
        SayHelloCommand = new DelegateCommand(SayHello)
            .ObservesCanExecute(() => CanSayHello); // <-- 修改此行
    }

    public bool CanSayHello // 添加此属性
    {
        get => _canSayHello;
        set => SetProperty(ref _canSayHello, value);
    }

    public DelegateCommand SayHelloCommand { get; }

    private void SayHello()
    {
        Title = "Hello Avalonia!";
    }
}
```

### CompositeCommand

`CompositeCommand` 是一个命令集合，可以将多个命令组合成一个命令，当执行该命令时，会依次执行集合中的命令。
当集合中的所有命令的 `CanExecute` 方法返回 `true` 时，`CompositeCommand` 的 `CanExecute` 方法才会返回 `true`。

```csharp
public class MainViewModel : BindableBase
{
    public MainViewModel()
    {
        CompositeCommand = new CompositeCommand();
        CompositeCommand.RegisterCommand(new DelegateCommand(SayHello));
        CompositeCommand.RegisterCommand(new DelegateCommand(SayGoodbye));
    }
    
    public CompositeCommand CompositeCommand { get; }

    private void SayHello()
    {
        Title = "Hello Avalonia!";
    }
    
    private void SayGoodbye()
    {
        Title = "Goodbye Avalonia!";
    }
}
```

如果视图与视图模型可能会被销毁，则也需要考虑符合命令的取消注册。

```csharp
SayHelloCompositeCommand.UnregisterCommand(new DelegateCommand(SayHello));
SayHelloCompositeCommand.UnregisterCommand(new DelegateCommand(SayGoodbye));
```

## 事件聚合器

Prism 中的事件聚合器是一个全局的事件总线，可以在不同的视图模型之间传递消息。

`GetEvent` 方法的泛型参数需要一个继承自 `EventBase` 的事件参数，框架提供了 `PubSubEvent<TPayload>` 可供直接使用。

`PubSubEvent<TPayload>` 是一个发布/订阅事件，可以通过 `Publish` 方法发布消息，通过 `Subscribe` 方法订阅消息。

如果需要操作 UI 元素，可以通过 `ThreadOption.UIThread` 来指定在 UI 线程上执行。

如果需要过滤指定的消息，可以通过 `Subscribe` 方法的的第四个参数 `Predicate<TPayload>` 来指定过滤条件。

```csharp
public class EventsViewModel : BindableBase, IDisposable
{
    private string _receivedMessage;
    private readonly SubscriptionToken _subscriptionToken;

    public EventsViewModel(IEventAggregator eventAggregator)
    {
        PublishEventCommand = new DelegateCommand(() =>
            eventAggregator.GetEvent<PubSubEvent<string>>()
                .Publish($"Hello from Avalonia! It's {DateTime.Now} now."))
        _subscriptionToken = eventAggregator.GetEvent<PubSubEvent<string>>()
            .Subscribe(message => ReceivedMessage = $"Received message: {message}", ThreadOption.UIThread);
    }

    public DelegateCommand PublishEventCommand { get; private set; }

    public string ReceivedMessage
    {
        get => _receivedMessage;
        private set => SetProperty(ref _receivedMessage, value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _subscriptionToken.Dispose();
    }
}
```

## 对话框服务

使用对话框服务前，需要在 `App.xaml.cs` 中注册对话框服务。

```csharp
containerRegistry.RegisterDialog<MessageBoxView, MessageBoxViewModel>();
```

Prism 中提供了 `IDialogService` 接口服务，可以用于显示对话框。

`dialogService.ShowDialog` 的其中一个重载可以接收视图名称、对话框参数、回调方法。

```csharp
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
```

为 `MessageBoxViewModel` 实现 `IDialogAware` 接口，可以让感知作为对话框数据模型的参数与状态。

`OnDialogOpened` 方法，`IDialogParameters parameters` 参数上可以获取 `ShowDialog` 所传入的 `parameters`，可用于设置属性。

`RequestClose` 事件可以用于关闭对话框，可以传入一个 `DialogResult` 枚举值，表示对话框的结果，以供 `ShowDialog` 的回调方法使用。

## 区域导航

Prism 中的区域导航主要由 `IRegionManager` 接口提供，可以用于将指定的区域导航到指定的视图。

要使用区域导航，首先需要在 `App.xaml.cs` 中注册区域导航服务。

```csharp
containerRegistry.RegisterForNavigation<ServicesView, ServicesViewModel>();
```

在视图中的 `ContentControl` 上添加 `prism:RegionManager.RegionName` 附加属性，指定区域名称。

```xaml
<ContentControl prism:RegionManager.RegionName="MainRegion" />
```

在视图模型中，通过 `IRegionManager.RequestNavigate` 方法导航到指定的视图。

```csharp
public class MainWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;

    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        _regionManager.RequestNavigate("MainRegion", nameof(ServicesView));
    }
}
```

即可让指定的区域导航至指定的视图。

`INavigationAware` 接口提供了参与导航的视图模型的相关信息。

- `OnNavigatedTo` 方法在视图模型导航到时调用，可以获取导航参数。
- `OnNavigatedFrom` 方法在视图模型导航离开时调用。
- `IsNavigationTarget` 方法用于指示当前视图模型是否可以处理导航请求。

在请求导航时可以传入参数：

```csharp
NavigationParameters parameters = new()
{
    { "at", DateTime.Now }
};
regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.NavigationsView), parameters);
```

`NavigationContext` 上便可获取到传入的参数。

```csharp
DateTime at = navigationContext.Parameters.GetValue<DateTime>("at");
```

`IRegionNavigationService` 上提供了 `IRegionNavigationJournal Journal` 属性，可以用于导航历史记录的管理。
`CanGoBack` 和 `CanGoForward` 属性，可以用于判断是否可以回退或前进。
`GoBack` 和 `GoForward` 方法，可以用于回退或前进。

如果需要让某些视图不被记录在导航历史记录中，例如启动页、登录页、对话框等中，可以为其实现 `IJournalAware`接口，并为 `PersistInHistory`
方法返回 `false`。

## 区域适配器

RegionManager.RegionName 所能附加的容器必须存在对应的区域适配器，否则 Prism 无法获知如何在容器中添加或删除视图。

内置的区域适配器有：
- `ContentControlRegionAdapter`
- `ItemsControlRegionAdapter`

而要在其它的容器上使用区域导航，需要自定义区域适配器。

```csharp
public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel>
{
    public StackPanelRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, StackPanel regionTarget)
    {
        region.Views.CollectionChanged += (sender, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                foreach (Control item in e.NewItems)
                    regionTarget.Children.Add(item);
            }

            if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            {
                foreach (Control item in e.OldItems)
                    regionTarget.Children.Remove(item);
            }
        };
    }

    protected override IRegion CreateRegion() => new SingleActiveRegion();
}
```

然后在 `App.xaml.cs` 中注册该区域适配器。

```csharp
protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
{
    base.ConfigureRegionAdapterMappings(regionAdapterMappings);
    regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
}
```

## 模块

模块是 Prism 中的一个概念，用于将应用程序分解为更小的功能单元，以便于管理和维护。

在模块的程序集内需要定义一个模块类，实现 `IModule` 接口。

```csharp
public class ModuleAModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
```

在模块的定义内，可以定于仅属于该模块的服务、视图、视图模型、导航等。这使得服务的定义不再混杂在主程序集中，而是更加清晰地分离开来。

```csharp
containerRegistry.RegisterForNavigation<ModuleAView, ModuleAViewModel>();
containerRegistry.Register<IDelayService, DelayService>();
```

在主程序集中，需要在 `App.xaml.cs` 中注册模块。

```csharp
protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
{
    base.ConfigureModuleCatalog(moduleCatalog);
    moduleCatalog.AddModule<ModuleAModule>();
}
```

模块相关的视图、视图模型、服务等会在模块初始化时被注册到容器中，以便于在模块内外使用。

如果不想让主程序包含对模块的引用，而是在程序启动时扫描目录加载模块，可以使用 `DirectoryModuleCatalog`。

```csharp
protected override IModuleCatalog CreateModuleCatalog()
{
    const string modulePath = @".\Modules";
    if (!Directory.Exists(modulePath))
        Directory.CreateDirectory(modulePath);
    return new DirectoryModuleCatalog { ModulePath = modulePath };
}
```

如果需要判断某个模块是否被加载，可以通过 `IModuleManager` 接口上提供的方法来判断。

```csharp
if (moduleManager.ModuleExists("ModuleAModule")
    && moduleManager.IsModuleInitialized("ModuleAModule"))
    regionManager.RequestNavigate(RegionNames.ContentRegion, "ModuleAView");、
```
