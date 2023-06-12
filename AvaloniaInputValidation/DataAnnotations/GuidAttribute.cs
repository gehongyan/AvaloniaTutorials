using System;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaInputValidation.DataAnnotations;

public class GuidAttribute : ValidationAttribute
{
    /// <inheritdoc />
    public override bool IsValid(object? value) =>
        value switch
        {
            Guid => true,
            string str => Guid.TryParse(str, out _),
            _ => false
        };

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        $"The {name} field is not a valid GUID.";
}
