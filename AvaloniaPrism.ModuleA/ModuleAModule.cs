using AvaloniaPrism.ModuleA.Services;
using AvaloniaPrism.ModuleA.ViewModels;
using AvaloniaPrism.ModuleA.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace AvaloniaPrism.ModuleA;

public class ModuleAModule : IModule
{
    /// <inheritdoc />
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ModuleAView, ModuleAViewModel>();
        containerRegistry.Register<IDelayService, DelayService>();
    }

    /// <inheritdoc />
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
