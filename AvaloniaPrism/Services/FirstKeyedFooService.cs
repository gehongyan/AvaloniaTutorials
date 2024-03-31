namespace AvaloniaPrism.Services;

public class FirstKeyedFooService : IKeyedFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} First IKeyedFooService";
}
