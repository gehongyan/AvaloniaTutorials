using Avalonia.Data.Converters;
using Sandbox.Models;

namespace Sandbox.ValueConverters;

public static class EnumConverters
{
    public static readonly IValueConverter Genders =
        new FuncValueConverter<Gender, string>(x => x switch
        {
            Gender.Male => "男",
            Gender.Female => "女",
            Gender.Other => "其他",
            _ => x.ToString()
        });

}
