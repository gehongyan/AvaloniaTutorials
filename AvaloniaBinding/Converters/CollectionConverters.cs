using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace AvaloniaBinding.Converters;

public static class CollectionConverters
{
    public static readonly IValueConverter Count =
        new FuncValueConverter<ICollection, int>(x => x?.Count ?? 0);

    public static readonly IValueConverter Index = new CollectionIndexConverter();

    private sealed class CollectionIndexConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is not IEnumerable enumerable)
                return -1;
            return enumerable.Cast<object>().ToList().IndexOf(value);
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int index
                || parameter is not IEnumerable enumerable
                || enumerable.Cast<object>().ToList() is not { Count: > 0 } list
                || list.Count <= index)
                return null;
            return list[index];
        }
    }
}
