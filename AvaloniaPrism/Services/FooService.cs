namespace AvaloniaPrism.Services;

public class FooService : IFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} IFooService";
}