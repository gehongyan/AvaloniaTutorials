using AvaloniaPrism.ModuleB.Services;
using AvaloniaPrism.ModuleB.ViewModels;
using AvaloniaPrism.ModuleB.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace AvaloniaPrism.ModuleB;

public class ModuleBModule : IModule
{
    /// <inheritdoc />
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ModuleBView, ModuleBViewModel>();
        containerRegistry.Register<IDelayService, DelayService>();
    }

    /// <inheritdoc />
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
