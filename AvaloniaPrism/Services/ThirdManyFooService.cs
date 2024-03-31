namespace AvaloniaPrism.Services;

public class ThirdManyFooService : IManyFooService
{
    public string GetSubtitle(string key) => $"This is the subtitle from {key} Third IManyFooService";
}