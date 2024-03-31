namespace AvaloniaPrism.Services;

public class ThirdKeyedFooService : IKeyedFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} Third IKeyedFooService";
}