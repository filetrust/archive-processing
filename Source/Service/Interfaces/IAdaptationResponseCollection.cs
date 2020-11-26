using Service.Enums;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Service.Interfaces
{
    public interface IAdaptationResponseCollection
    {
        bool IsCompleted { get; }
        void Add(KeyValuePair<Guid, AdaptationOutcome> response);
        void CompleteAdding();
        bool TryTake(out KeyValuePair<Guid, AdaptationOutcome> outcome, CancellationToken token);
    }
}
