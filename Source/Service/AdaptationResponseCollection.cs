using Service.Enums;
using Service.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Service
{
    public class AdaptationResponseCollection : IAdaptationResponseCollection
    {
        private BlockingCollection<KeyValuePair<Guid, AdaptationOutcome>> _collection = new BlockingCollection<KeyValuePair<Guid, AdaptationOutcome>>();

        public bool IsCompleted => _collection.IsCompleted;

        public void Add(KeyValuePair<Guid, AdaptationOutcome> response)
        {
            _collection.Add(response);
        }

        public void CompleteAdding()
        {
            _collection.CompleteAdding();
        }

        public KeyValuePair<Guid, AdaptationOutcome> Take(CancellationToken token)
        {
            return _collection.Take(token);
        }
    }
}
