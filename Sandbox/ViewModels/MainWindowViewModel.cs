using System.Collections.ObjectModel;

namespace Sandbox.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
    }

    public ObservableCollection<CodeDescriptor> Items { get; set; } =
    [
        new("Item 1", "Value 1"),
        new("Item 2", "Value 2"),
        new("Item 3", "Value 3")
    ];
}

public record CodeDescriptor(string Code, string Value);