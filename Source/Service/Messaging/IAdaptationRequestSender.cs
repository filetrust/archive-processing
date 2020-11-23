using Service.Enums;
using System.Threading;

namespace Service.Messaging
{
    public interface IAdaptationRequestSender
    {
        AdaptationOutcome Send(string fileId, string originalStoreFilePath, string rebuiltStoreFilePath, CancellationToken processingCancellationToken);
    }
}
