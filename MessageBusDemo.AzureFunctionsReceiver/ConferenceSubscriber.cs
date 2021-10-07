using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBusDemo.AzureFunctionSender;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MessageBusDemo.AzureFunctionsReceiver
{
    public class ConferenceSubscriber
    {
        [FunctionName("ConferenceSubscriber")]
        public async Task Run(
            [ServiceBusTrigger("conferences","mysubscription", Connection = "AzureWebJobsServiceBus")]
            Conference conference,
            int deliveryCount,
            MessageReceiver messageReceiver,
            string lockToken,
            ILogger log)
        {
            try
            {
                log.LogInformation(
                    $"Thank you. We will attend the conference #{conference.ConferenceId} on room {conference.RoomName}");

                switch (conference.RoomName)
                {
                    case "Chemical": throw new PoisonException("We are in the chemical room");
                    case "Lazy":
                        await ResourceSimulator(log);
                        break;
                    case "Phantom": throw new ArgumentException("Phantom is not allowed");
                    default:
                        log.LogInformation("Common conference");
                        break;
                }

                log.LogInformation($"Message processing was COMPLETED!");
                await messageReceiver.CompleteAsync(lockToken);
            }
            catch (PoisonException e)
            {
                await messageReceiver.DeadLetterAsync(lockToken, e.Message, e.ToString());
                log.LogInformation($"Message was explicitly dead-lettered as poison message based on lockToken {lockToken}");
            }
            catch (Exception e)
            {
                log.LogInformation($"An error occurred on delivery #{deliveryCount}");
                log.LogError(e, e.Message);

                if (deliveryCount == 3)
                {
                    log.LogInformation($"We will explicitly dead-lettering based on lockToken {lockToken}");
                    await messageReceiver.DeadLetterAsync(lockToken, e.Message, e.ToString());
                }
                else
                {
                    // throw vs AbandonAsync discussion
                    //await messageReceiver.AbandonAsync(lockToken);
                    //throw;
                }
            }
        }

        private async Task ResourceSimulator(ILogger logger)
        {
            logger.LogInformation("We are waiting for the lazy attender");
            await Task.Delay(20000);
        }
    }
}
