using System;
using System.Threading.Tasks;
using MassTransit;
using MessageBusDemo.CommonHelpers;
using MessageBusDemo.CommonHelpers.Contracts;

namespace MessageBusDemo.MassTransit.Receiver
{
    public class DemoConsumer : IConsumer<DemoMessage>
    {
        public Task Consume(ConsumeContext<DemoMessage> context)
        {
            Utils.WriteLine($"Received message {context.Message.Content}", ConsoleColor.DarkGreen);

            var content = context.Message.Content;

            if (content.StartsWith("Poison"))
            {
                throw new Exception("The message is malformed");
            }

            Utils.WriteLine($"Completed consumption for message {content}", ConsoleColor.DarkGreen);
            
            return Task.CompletedTask;
        }
    }
}