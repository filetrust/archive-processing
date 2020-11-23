using Service.Enums;
using System.Collections.Generic;

namespace Service.Messaging
{
    public interface IResponseProcessor
    {
        AdaptationOutcome Process(IDictionary<string, object> headers, byte[] body);
    }
}