using System.Collections.Specialized;
using Avalonia.Controls;
using Prism.Regions;

namespace AvaloniaPrism.RegionAdapters;

public class GridRegionAdapter : RegionAdapterBase<Grid>
{
    public GridRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, Grid regionTarget)
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
