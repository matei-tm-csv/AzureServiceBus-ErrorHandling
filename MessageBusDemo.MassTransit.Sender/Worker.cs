using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MessageBusDemo.CommonHelpers;
using MessageBusDemo.CommonHelpers.Contracts;
using Microsoft.Extensions.Hosting;

namespace MessageBusDemo.MassTransit.Sender
{
    public class Worker : IHostedService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public Worker(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Console.WriteLine("[t]ext/[p]oison/e[x]it?");

                var messageType = Console.ReadLine()?.ToLower();

                if (messageType == "exit" || messageType == "x")
                {
                    break;
                }

                switch (messageType)
                {
                    case "t":
                    case "text":
                        await SendMessage("Hello");
                        break;
                    case "p":
                    case "poison":
                        await SendMessage("Poison");
                        break;
                    default:
                        Console.WriteLine("What?");
                        break;
                }
            }
        }

        private async Task SendMessage(string content)
        {
            await _publishEndpoint
                .Publish(
                    new DemoMessage()
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Content = content
                    }
                );

            Utils.WriteLine("Message published with MassTransit", ConsoleColor.Cyan);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}