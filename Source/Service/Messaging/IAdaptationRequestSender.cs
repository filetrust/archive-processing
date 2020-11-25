using System.Threading;

namespace Service.Messaging
{
    public interface IAdaptationRequestSender
    {
        int ExpectedMessageCount { set; }
        void Send(string fileId, string originalStoreFilePath, string rebuiltStoreFilePath, CancellationToken processingCancellationToken);
    }
}
