using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AvaloniaInputValidation.Converters;

public class IntIsCheckedConverter : IValueConverter
{
    public static readonly IntIsCheckedConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
{
        if (parameter is not int reference)
            reference = System.Convert.ToInt32(parameter);
        return value is int intValue && reference == intValue;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isChecked)
            return BindingOperations.DoNothing;
        return isChecked
            ? System.Convert.ToInt32(parameter)
            : BindingOperations.DoNothing;
    }
}
