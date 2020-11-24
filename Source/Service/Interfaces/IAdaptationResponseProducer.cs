using System.Threading;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAdaptationResponseProducer
    {
        Task SendMessages(string originalDirectory, string rebuiltDirectory, CancellationToken token);
    }
}
