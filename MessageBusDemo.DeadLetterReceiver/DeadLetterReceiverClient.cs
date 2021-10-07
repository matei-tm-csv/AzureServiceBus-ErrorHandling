using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBusDemo.CommonHelpers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageBusDemo.DeadLetterReceiver
{
    static class DeadLetterReceiverClient
    {
        private static MessageReceiver _messageReceiver;

        private static void Main(string[] args)
        {
            Utils.WriteLine("DeadLetterReceiverConsole", ConsoleColor.White);
            Console.WriteLine();

            Thread.Sleep(3000);


            var deadLetterPath = EntityNameHelper.FormatDeadLetterPath(Settings.QueuePath);

            Utils.WriteLine($"Dead letter queue path { deadLetterPath }", ConsoleColor.Cyan);

            _messageReceiver = new MessageReceiver(Settings.ConnectionString, deadLetterPath);

            ReceiveDeadLetterMessages();

            Utils.WriteLine("Receiving dead letter messages", ConsoleColor.Cyan);
            Console.WriteLine();

            Console.ReadLine();
        }

        private static void ReceiveDeadLetterMessages()
        {
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = true
            };

            _messageReceiver.RegisterMessageHandler(ProcessDeadLetterMessage, options);

            Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);

        }

        private static Task ProcessDeadLetterMessage(Message message, CancellationToken token)
        {
            Utils.WriteLine("Received dead letter message", ConsoleColor.Cyan);
            Utils.WriteLine($"    Content type: { message.ContentType }", ConsoleColor.Green);
            Utils.WriteLine($"    DeadLetterReason: { message.UserProperties["DeadLetterReason"] }", ConsoleColor.Green);
            Utils.WriteLine($"    DeadLetterErrorDescription: { message.UserProperties["DeadLetterErrorDescription"] }", ConsoleColor.Green);

            Console.WriteLine();
            return Task.CompletedTask;
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Utils.WriteLine($"Exception: { arg.Exception.Message }", ConsoleColor.Yellow);
            return Task.CompletedTask;
        }
    }
}
