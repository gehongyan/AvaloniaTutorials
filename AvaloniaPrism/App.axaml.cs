using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaPrism.ModuleA;
using AvaloniaPrism.RegionAdapters;
using AvaloniaPrism.Services;
using AvaloniaPrism.ViewModels;
using AvaloniaPrism.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace AvaloniaPrism;

// 2. 更改 App 所继承的基类，从 Application 到 PrismApplication
public partial class App : PrismApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // 5. 在 Initialize 方法中初始化 Prism 应用程序
        base.Initialize();
    }

    // public override void OnFrameworkInitializationCompleted()
    // {
    //     if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //     {
    //         desktop.MainWindow = new MainWindow
    //         {
    //             DataContext = new MainWindowViewModel(),
    //         };
    //     }
    //
    //     base.OnFrameworkInitializationCompleted();
    // }

    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 如果不启用自动关联，则需要手动为视图注册视图模型
        // ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
        // ViewModelLocationProvider.Register<ServicesView, ServicesViewModel>();
        // ViewModelLocationProvider.Register<CommandsView, CommandsViewModel>();
        // ViewModelLocationProvider.Register<DialogsView, DialogsViewModel>();
        // ViewModelLocationProvider.Register<EventsView, EventsViewModel>();
        // ViewModelLocationProvider.Register(typeof(MainView).FullName, () => new MainViewModel());

        // 注册服务
        containerRegistry.Register<IFooService, FooService>();
        // containerRegistry.RegisterSingleton<IFooService, FooService>();
        // containerRegistry.RegisterScoped<IFooService, FooService>();
        // containerRegistry.RegisterInstance<IFooService>(new FooService());

        // 注册服务的多个实现
        containerRegistry.Register<IManyFooService, FirstManyFooService>();
        containerRegistry.Register<IManyFooService, SecondManyFooService>();
        containerRegistry.Register<IManyFooService, ThirdManyFooService>();

        // 带有名称地注册服务地实现，这是 Prism.Ioc 上提供的方法，name 需要为字符串
        containerRegistry.Register<IKeyedFooService, FirstKeyedFooService>("First");
        containerRegistry.Register<IKeyedFooService, SecondKeyedFooService>("Second");
        containerRegistry.Register<IKeyedFooService, ThirdKeyedFooService>("Third");
        // 带有键控地注册服务，这是 DryIoc 上提供地方法，serviceKey 接受任意 object
        // containerRegistry.GetContainer().Register<IKeyedFooService, FirstKeyedFooService>(serviceKey: "First");
        // containerRegistry.GetContainer().Register<IKeyedFooService, SecondKeyedFooService>(serviceKey: "Second");
        // containerRegistry.GetContainer().Register<IKeyedFooService, ThirdKeyedFooService>(serviceKey: "Third");

        // 注册对话框
        containerRegistry.RegisterDialog<MessageBoxView, MessageBoxViewModel>();
        containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

        // 注册导航
        containerRegistry.RegisterForNavigation<ServicesView, ServicesViewModel>();
        containerRegistry.RegisterForNavigation<CommandsView, CommandsViewModel>();
        containerRegistry.RegisterForNavigation<DialogsView, DialogsViewModel>();
        containerRegistry.RegisterForNavigation<EventsView, EventsViewModel>();
        containerRegistry.RegisterForNavigation<NavigationsView, NavigationsViewModel>();
    }

    /// <inheritdoc />
    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
        regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
        regionAdapterMappings.RegisterMapping(typeof(Grid), Container.Resolve<GridRegionAdapter>());
    }

    /// <inheritdoc />
    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <inheritdoc />
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);
        moduleCatalog.AddModule<ModuleAModule>();
    }

    /// <inheritdoc />
    protected override IModuleCatalog CreateModuleCatalog()
    {
        const string modulePath = @".\Modules";
        if (!Directory.Exists(modulePath))
            Directory.CreateDirectory(modulePath);
        return new DirectoryModuleCatalog { ModulePath = modulePath };
    }

    /// <inheritdoc />
    protected override void InitializeModules()
    {
        IModuleManager? manager = Container.Resolve<IModuleManager>();
        manager.LoadModuleCompleted += (sender, args) =>
        {
            if (args.Error is ContainerResolutionException exception)
            {
                ContainerResolutionErrorCollection errors = exception.GetErrors();
                foreach((Type type, Exception ex) in errors)
                {
                    Console.WriteLine($"Error with: {type.FullName}");
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                }
            }
        };
        manager.Run();
    }
}
