using System;
using Avalonia.Markup.Xaml;

namespace FluentIconOverview.MarkupExtensions;

public class EnumValuesExtension<T> : MarkupExtension
    where T : struct, Enum
{
    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues<T>();
}
