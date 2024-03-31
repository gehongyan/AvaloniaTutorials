namespace AvaloniaPrism.Services;

public class SecondKeyedFooService : IKeyedFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} Second IKeyedFooService";
}