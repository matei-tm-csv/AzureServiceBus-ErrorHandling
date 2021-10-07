using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageBusDemo.CommonHelpers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageBusDemo.Sender
{
    internal static class SenderClient
    {
        private static MessageSender _messageSender;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Sender Console");
            Console.WriteLine();

            Thread.Sleep(3000);

            _messageSender = new MessageSender(Settings.ConnectionString, Settings.QueuePath);

            while (true)
            {
                Console.WriteLine("[t]ext/[j]son/[p]oison/[u]nknown/[m]arkdeferred/[a]nnouncedeferred/[r]esourcemanager/e[x]it/[h]elp?");

                var messageType = Console.ReadLine()?.ToLower();

                if (messageType == "exit" || messageType == "x")
                {
                    break;
                }

                switch (messageType)
                {
                    case "t":
                    case "text":
                        await SendMessage("Hello", "text/plain");
                        break;
                    case "j":
                    case "json":
                        await SendMessage("{\"contact\": {\"name\": \"John\",\"twitter\": \"@johndoe\" }}", "application/json");
                        break;
                    case "p":
                    case "poison":
                        await SendMessage("<contact><name>John</name><twitter>@johndoe</twitter></contact>", "application/json");
                        break;
                    case "m":
                    case "markdeferred":
                        await SendMessage("I will be late", "application/complex");
                        break;
                    case "a":
                    case "announcedeferred":
                        await EnterDeferredSequence();
                        break;
                    case "r":
                    case "resourcemanager":
                        await SimulateResourceManagement();
                        break;
                    case "h":
                    case "help":
                        ShowHelpInfo();
                        break;
                    case "u":
                    case "unknown":
                        await SendMessage("Unknown message", "application/unknown");
                        break;

                    default:
                        Console.WriteLine("What?");
                        break;
                }
            }

            await _messageSender.CloseAsync();
        }

        private static async Task SimulateResourceManagement()
        {
            Console.WriteLine("[e]nable/[d]isable the mock resource required by text messages processor");

            var switchCommand = Console.ReadLine()?.ToLower();

            switch (switchCommand)
            {
                case "d":
                    await ServiceBusManager.DisableMockResourceQueue(); break;
                case "e":
                    await ServiceBusManager.EnableMockResourceQueue(); break;
            }
        }

        private static void ShowHelpInfo()
        {
            Console.WriteLine(@"Text - demonstrates how to explicit dead-letter, after a number of retries, when a dependent resource is not available 
Json - send a clean Json message
Poison - send a Poison message with Json contentType but XML body,
Markdeferred - send a message that will be deferred by the receiver
Announcedeferred - send a message that will announce the receiver to process and complete a deferred message
Resourcemanager - enable or disable the mock resource"
            );
        }

        private static async Task EnterDeferredSequence()
        {
            Console.WriteLine("Sequence number?");

            var sequenceNumber = Console.ReadLine()?.ToLower();
            await SendMessage(SequenceNumberHelper.Encode(sequenceNumber), "application/announcer");
        }

        static async Task SendMessage(string text, string contentType)
        {

            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(text))
                {
                    ContentType = contentType
                };
                Utils.WriteLine($"Created Message: { text }", ConsoleColor.Cyan);

                await _messageSender.SendAsync(message);
                Utils.WriteLine("Sent Message", ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                Utils.WriteLine(ex.Message, ConsoleColor.Yellow);
            }
        }
    }
}
