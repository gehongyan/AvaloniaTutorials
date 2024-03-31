namespace AvaloniaPrism.Services;

public class FirstManyFooService : IManyFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} First IManyFooService";
}