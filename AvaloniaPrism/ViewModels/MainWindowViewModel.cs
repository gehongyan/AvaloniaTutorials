using System;
using System.Windows.Input;
using AvaloniaPrism.Core.Constants;
using Prism.Commands;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace AvaloniaPrism.ViewModels;

public class MainWindowViewModel
{
    public MainWindowViewModel(IRegionManager regionManager,
        IDialogService dialogService,
        IModuleManager moduleManager)
    {
        ServicesCommand = new DelegateCommand(() =>
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.ServicesView)));
        CommandsCommand = new DelegateCommand(() =>
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.CommandsView)));
        EventsCommand = new DelegateCommand(() =>
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.EventsView)));
        DialogsCommand = new DelegateCommand(() =>
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.DialogsView)));
        NavigationsCommand = new DelegateCommand(() =>
        {
            NavigationParameters parameters = new()
            {
                { "at", DateTime.Now }
            };
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(Views.NavigationsView), parameters);
        });

        ModuleACommand = new DelegateCommand(() =>
            regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(ModuleA.Views.ModuleAView)));

        ModuleBCommand = new DelegateCommand(() =>
            {
                if (moduleManager.ModuleExists("ModuleBModule")
                    && moduleManager.IsModuleInitialized("ModuleBModule"))
                    regionManager.RequestNavigate(RegionNames.ContentRegion, "ModuleBView");
                else
                    dialogService.ShowDialog(nameof(Views.MessageBoxView), new DialogParameters
                    {
                        { "title", "Error" },
                        { "content", "Module B is not loaded" }
                    });
            }
        );
    }

    public ICommand ServicesCommand { get; set; }

    public ICommand CommandsCommand { get; set; }

    public ICommand EventsCommand { get; set; }

    public ICommand DialogsCommand { get; set; }

    public ICommand NavigationsCommand { get; set; }

    public ICommand ModuleACommand { get; set; }

    public ICommand ModuleBCommand { get; set; }
}
