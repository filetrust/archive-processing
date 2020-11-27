using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAdaptationResponseConsumer
    {
        void SetPendingFiles(IEnumerable<Guid> fileIds);
        Task ConsumeResponses(IDictionary<Guid, string> fileMappings, string rebuiltDir, string originalDir, CancellationToken token);
    }
}
