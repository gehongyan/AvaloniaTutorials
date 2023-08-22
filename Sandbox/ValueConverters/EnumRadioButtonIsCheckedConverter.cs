using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Sandbox.ValueConverters;

public class EnumRadioButtonIsCheckedConverter : MarkupExtension, IValueConverter
{
    public EnumRadioButtonIsCheckedConverter(object value)
    {
        Value = value;
    }

    [ConstructorArgument(nameof(Value))]
    public object Value { get; set; }

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Equals(value, Value);

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Value : BindingOperations.DoNothing;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
