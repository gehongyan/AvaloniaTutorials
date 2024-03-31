using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaPrism.Services;
using DryIoc;
using Prism.Ioc;

namespace AvaloniaPrism.ViewModels;

public class ServicesViewModel
{
    public ServicesViewModel(
        IFooService fooService,
        Func<IFooService> fooServiceFunc,
        Lazy<IFooService> fooServiceLazy,
        IEnumerable<IManyFooService> manyFooServices,
        // 由 DryIoc 提供
        IResolver resolver,
        // 由 Prism.Ioc 提供
        IContainerProvider containerProvider
    )
    {
        // 直接解析
        DirectSubtitle = fooService.GetSubtitle("Direct");
        // 解析为 Func
        FuncSubtitle = fooServiceFunc().GetSubtitle("Func");
        // 解析为 Lazy
        LazySubtitle = fooServiceLazy.Value.GetSubtitle("Lazy");

        // 解析全部实现
        IEnumerable<string> subtitles = manyFooServices.Select(s => s.GetSubtitle("ManyFooServices"));
        ManyFooServicesSubtitle = string.Join(Environment.NewLine, subtitles);

        // 名称或键控解析
        IKeyedFooService firstFooService = resolver.Resolve<IKeyedFooService>("First");
        KeyedFirstSubtitle = firstFooService.GetSubtitle("KeyedFooService");
        IKeyedFooService secondFooService = containerProvider.Resolve<IKeyedFooService>("Second");
        KeyedSecondSubtitle = secondFooService.GetSubtitle("KeyedFooService");
    }

    public string DirectSubtitle { get; set; }

    public string FuncSubtitle { get; set; }

    public string LazySubtitle { get; set; }

    public string ManyFooServicesSubtitle { get; set; }

    public string KeyedFirstSubtitle { get; set; }

    public string KeyedSecondSubtitle { get; set; }
}
