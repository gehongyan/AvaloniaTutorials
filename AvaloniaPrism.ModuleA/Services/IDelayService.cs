using System.Threading.Tasks;

namespace AvaloniaPrism.ModuleA.Services;

public interface IDelayService
{
    Task DelayAsync(int milliseconds);
}
