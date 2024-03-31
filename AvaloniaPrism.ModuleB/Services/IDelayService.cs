using System.Threading.Tasks;

namespace AvaloniaPrism.ModuleB.Services;

public interface IDelayService
{
    Task DelayAsync(int milliseconds);
}
