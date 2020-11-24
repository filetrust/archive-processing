using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAdaptationResponseConsumer
    {
        Task ConsumeResponses(IDictionary<string, string> fileMappings, string rebuiltDir, string originalDir, CancellationToken token);
    }
}
