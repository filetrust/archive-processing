using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Service.Configuration;
using Service.Interfaces;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            var startup = new Startup();
            startup.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Get Service and call method
            using (var scope = serviceProvider.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetService<IArchiveProcessorConfig>();

                var pusher = new MetricPusher(new MetricPusherOptions
                {
                    Endpoint = configuration.MetricsEndpoint,
                    Job = "archive-processing",
                    IntervalMilliseconds = 5,
                    Instance = configuration.ArchiveFileId
                });

                pusher.Start();

                var service = scope.ServiceProvider.GetService<IArchiveProcessor>();
                service.Process();

                pusher.Stop();
            }
        }
    }
}
