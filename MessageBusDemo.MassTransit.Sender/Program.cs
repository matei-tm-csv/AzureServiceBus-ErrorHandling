using System.Threading.Tasks;
using MassTransit;
using MessageBusDemo.CommonHelpers;
using MessageBusDemo.CommonHelpers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace MessageBusDemo.MassTransit.Sender
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(services =>
            {
                services.AddMassTransit(config => config.UsingAzureServiceBus((context, busConfig) =>
                                   {
                                       busConfig.Host(Settings.ConnectionString);
                                       var errorHandlingDemoTopic = Settings.TopicPath;

                                       busConfig.Message<DemoMessage>(configTopology => configTopology.SetEntityName(errorHandlingDemoTopic));
                                   })
                                   );

                services.AddHostedService<Worker>();
            });
    }
}
