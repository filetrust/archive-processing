using Service.Enums;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Service.Interfaces
{
    public interface IAdaptationResponseCollection
    {
        void Add(KeyValuePair<Guid, AdaptationOutcome> response);
        void CompleteAdding();
        KeyValuePair<Guid, AdaptationOutcome> Take(CancellationToken token);
    }
}
