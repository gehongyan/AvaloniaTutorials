using System.Threading.Tasks;

namespace AvaloniaPrism.ModuleB.Services;

public class DelayService : IDelayService
{
    public async Task DelayAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
}
