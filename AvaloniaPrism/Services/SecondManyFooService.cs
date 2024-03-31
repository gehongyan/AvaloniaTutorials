namespace AvaloniaPrism.Services;

public class SecondManyFooService : IManyFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} Second IManyFooService";
}