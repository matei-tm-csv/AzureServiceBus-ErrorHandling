using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageBusDemo.CommonHelpers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace MessageBusDemo.Receiver
{
    internal static partial class ReceiverClient
    {
        private static MessageReceiver _messageReceiver;


        private static async Task Main(string[] args)
        {
            Utils.WriteLine("ReceiverConsole", ConsoleColor.White);
            Console.WriteLine();

            await ServiceBusManager.CreateDemoQueues();

            _messageReceiver = new MessageReceiver(Settings.ConnectionString, Settings.QueuePath);

            ReceiveMessages();

            Console.ReadLine();
        }

        private static void ReceiveMessages()
        {
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _messageReceiver.RegisterMessageHandler(ProcessMessage, options);

            Utils.WriteLine("Receiving messages", ConsoleColor.Cyan);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Utils.WriteLine($"Exception: {arg.Exception.Message}", ConsoleColor.Yellow);

            return Task.CompletedTask;
        }

        private static async Task ProcessMessage(Message message, CancellationToken token)
        {
            Utils.WriteLine("Received: " + message.ContentType, ConsoleColor.Cyan);

            switch (message.ContentType)
            {
                case "text/plain":
                    await ProcessTextMessage(message);
                    break;
                case "application/json":
                    await ProcessJsonMessage(message);
                    break;
                case "application/complex":
                    await ProcessComplexMessage(message);
                    break;
                case "application/announcer":
                    await ProcessAnnouncerMessage(message);
                    break;
                default:
                    Console.WriteLine("Received unknown message: " + message.ContentType);

                    await _messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, "Unknown message type",
                        "The message type: " + message.ContentType + " is not known.");

                    break;
            }
        }

        /// <summary>
        /// The method demonstrates the consequences of using Deferred messages
        /// The deferred message will stay in the queue until a client will complete it
        /// Extra properties can be attached in order to motivate the deferral
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task ProcessComplexMessage(Message message)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            Utils.WriteLine($"Text message: {body} - DeliveryCount: {message.SystemProperties.DeliveryCount}",
                ConsoleColor.Green);

            try
            {
                Utils.WriteLine($"Message will be deferred. Sequence id: {message.SystemProperties.SequenceNumber}",
                    ConsoleColor.DarkMagenta);


                await _messageReceiver.DeferAsync(
                    message.SystemProperties.LockToken,
                    new Dictionary<string, object>()
                    {
                            { "ExtraProperty01", "Hera is coming late" },
                            { "ExtraProperty02", "Hecate is coming late" }
                    });

            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

                if (message.SystemProperties.DeliveryCount > Settings.MaxProcessingCount)
                {
                    await _messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message,
                        ex.ToString());
                }
            }
        }

        /// <summary>
        /// Demonstrates how the DeferredMessage is retrieved and completed
        /// Uses an announcerMessage as the carrier fro deferred messages sequence number
        /// </summary>
        /// <param name="announcerMessage"></param>
        /// <returns></returns>
        private static async Task ProcessAnnouncerMessage(Message announcerMessage)
        {
            var body = Encoding.UTF8.GetString(announcerMessage.Body);

            Utils.WriteLine($"Text message: {body} - DeliveryCount: {announcerMessage.SystemProperties.DeliveryCount}",
                ConsoleColor.Green);

            try
            {
                var sequenceNumber = SequenceNumberHelper.Decode(body);

                var deferredMessage = await _messageReceiver.ReceiveDeferredMessageAsync(sequenceNumber);
                var deferredMessageBody = Encoding.UTF8.GetString(deferredMessage.Body);
                Utils.WriteLine(
                    $"Deferred Text message: {deferredMessageBody} - DeliveryCount: {deferredMessage.SystemProperties.DeliveryCount}",
                    ConsoleColor.Yellow);

                await _messageReceiver.CompleteAsync(deferredMessage.SystemProperties.LockToken);
                Utils.WriteLine("Completed the processing of deferred messages", ConsoleColor.Cyan);

                await _messageReceiver.CompleteAsync(announcerMessage.SystemProperties.LockToken);
                Utils.WriteLine("Processed announcer and the deferred messages", ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

                if (announcerMessage.SystemProperties.DeliveryCount > Settings.MaxProcessingCount)
                {
                    await _messageReceiver.DeadLetterAsync(announcerMessage.SystemProperties.LockToken, ex.Message,
                        ex.ToString());
                    Utils.WriteLine($"Message was explicitly dead-lettered after {Settings.MaxProcessingCount} retries",
                        ConsoleColor.DarkMagenta);
                }
            }
        }

        /// <summary>
        /// The method demonstrates how to explicit dead-letter a message that depends on resources
        /// If the DeliveryCount does not exceed the MaxProcessingCount the message will be visible on the queue after the LockTimeout expiration
        /// In order to simulate an unavailable resource, disable the ForwardingQueuePath and verify the behavior
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task ProcessTextMessage(Message message)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            Utils.WriteLine($"Text message: {body} - DeliveryCount: {message.SystemProperties.DeliveryCount}",
                ConsoleColor.Green);

            try
            {
                await ForwardToExternalResource();

                await _messageReceiver.CompleteAsync(message.SystemProperties.LockToken);

                Utils.WriteLine("Processed message", ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

                if (message.SystemProperties.DeliveryCount > Settings.MaxProcessingCount)
                {
                    await _messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message,
                        ex.ToString());
                    Utils.WriteLine($"Message was explicitly dead-lettered after {Settings.MaxProcessingCount} retries",
                        ConsoleColor.DarkMagenta);
                }
            }
        }

        /// <summary>
        /// The method demonstrates how to handle a Poison message
        /// For particular exceptions we can decide if it is poison message.
        /// In order to simulate it send a message with application/json contentType containing a malformed json body 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task ProcessJsonMessage(Message message)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            Utils.WriteLine($"JSON message {body}" + body, ConsoleColor.Green);

            try
            {
                dynamic data = JsonConvert.DeserializeObject(body);
                Utils.WriteLine($"      Name: {data.contact.name}", ConsoleColor.Green);
                Utils.WriteLine($"      Twitter: {data.contact.twitter}", ConsoleColor.Green);

                await _messageReceiver.CompleteAsync(message.SystemProperties.LockToken);

                Utils.WriteLine("Processed message", ConsoleColor.Cyan);
            }
            catch (JsonReaderException ex)
            {
                Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

                await _messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message, ex.ToString());
                Utils.WriteLine("Message was explicitly dead-lettered as poison message",
                    ConsoleColor.DarkMagenta);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Exception: {ex.Message}", ConsoleColor.Yellow);

                if (message.SystemProperties.DeliveryCount > Settings.MaxProcessingCount)
                {
                    await _messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, ex.Message,
                        ex.ToString());
                    Utils.WriteLine($"Message was explicitly dead-lettered after {Settings.MaxProcessingCount} retries",
                        ConsoleColor.DarkMagenta);
                }
            }
        }

        /// <summary>
        /// The mock external resource is another queue. On that queue we can simulate transient faults by activating/deactivating it.
        /// </summary>
        /// <returns></returns>
        private static async Task ForwardToExternalResource()
        {
            var forwardingMessage = new Message();
            var forwardingQueueClient = new QueueClient(Settings.ConnectionString, Settings.ForwardingQueuePath);
            await forwardingQueueClient.SendAsync(forwardingMessage);
            await forwardingQueueClient.CloseAsync();
        }

        
    }
}