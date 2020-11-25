using Service.Enums;
using System;
using System.Collections.Generic;

namespace Service.Messaging
{
    public interface IResponseProcessor
    {
        KeyValuePair<Guid, AdaptationOutcome> Process(IDictionary<string, object> headers);
    }
}