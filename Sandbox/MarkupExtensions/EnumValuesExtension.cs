using System;
using Avalonia.Markup.Xaml;

namespace Sandbox.MarkupExtensions;

public class EnumValuesExtension : MarkupExtension
{
    public EnumValuesExtension(Type type)
    {
        Type = type;
    }

    [ConstructorArgument("type")]
    public Type Type { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues(Type);
}
