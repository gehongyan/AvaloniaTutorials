using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaBinding.Controls;

/// <summary>
///     A TextBox that provides OneWayText property.
/// </summary>
public class OneWayTextBox : TextBox
{
    /// <summary>
    /// Defines the <see cref="OneWayText"/> property
    /// </summary>
    public static readonly StyledProperty<string?> OneWayTextProperty =
        AvaloniaProperty.Register<TextBox, string?>(nameof(OneWayText));

    /// <summary>
    /// Gets or sets the OneWayText content of the TextBox
    /// </summary>
    public string? OneWayText
    {
        get => GetValue(OneWayTextProperty);
        set => SetValue(OneWayTextProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TextBox);
}
