using Service.Enums;
using System;
using System.Collections.Generic;

namespace Service.Interfaces
{
    public interface IAdaptationResponseCollection
    {
        bool IsCompleted { get; }
        void Add(KeyValuePair<Guid, AdaptationOutcome> response);
        void CompleteAdding();
        KeyValuePair<Guid, AdaptationOutcome> Take();
    }
}
