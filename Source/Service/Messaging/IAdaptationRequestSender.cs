using Service.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Service.Messaging
{
    public interface IAdaptationRequestSender
    {
        void Send(string fileId, string originalStoreFilePath, string rebuiltStoreFilePath, CancellationToken processingCancellationToken);
    }
}
