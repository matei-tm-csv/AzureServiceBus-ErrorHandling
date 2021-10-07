using System;
using System.Threading.Tasks;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MessageBusDemo.CommonHelpers;
using MessageBusDemo.CommonHelpers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageBusDemo.MassTransit.Receiver
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(services =>
            {
                services.AddMassTransit
                (
                    config =>
                    {
                        {
                            config.AddConsumer<DemoConsumer>();
                            config.UsingAzureServiceBus((context, busConfig) =>
                               {
                                   busConfig.Host(Settings.ConnectionString);
                                   var errorHandlingDemoTopic = Settings.TopicPath;
                                   var subscriptionName = $"{errorHandlingDemoTopic}-consumer01";

                                   busConfig.Message<DemoMessage>(m => m.SetEntityName(errorHandlingDemoTopic));

                                   busConfig.SubscriptionEndpoint<DemoMessage>(subscriptionName, e =>
                                   {
                                       e.UseMessageRetry(r => ConfigRetryStrategy(r));
                                       e.ConfigureConsumer<DemoConsumer>(context);
                                       // Send failures to built-in Azure Service Bus Dead Letter queue
                                       e.ConfigureDeadLetterQueueDeadLetterTransport();
                                       e.ConfigureDeadLetterQueueErrorTransport();
                                       e.EnableDeadLetteringOnMessageExpiration = true;
                                   });
                               }
                        );
                        }
                    });

                services.AddHostedService<Worker>();
            });

        /// <summary>
        /// Check
        /// https://masstransit-project.com/usage/exceptions.html#retry
        /// for retries scenarios
        /// </summary>
        /// <param name="r"></param>
        private static void ConfigRetryStrategy(IRetryConfigurator r)
        {
            r.Immediate(5);
        }
    }
}
