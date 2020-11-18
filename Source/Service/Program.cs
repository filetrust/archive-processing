using System;
using Microsoft.Extensions.DependencyInjection;

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
                var service = scope.ServiceProvider.GetService<IArchiveProcessor>();
                service.Process();
            }
        }
    }
}
