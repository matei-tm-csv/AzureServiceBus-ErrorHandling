using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MessageBusDemo.AzureFunctionSender
{
    public static class ConferenceAnnouncement
    {
        [FunctionName("ConferenceAnnouncement")]
        [return: ServiceBus("conferences", Connection = "AzureWebJobsServiceBus")]
        public static async Task<Conference> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest httpRequest,
            ILogger log)
        {
            log.LogInformation("New conference announcement!");

            var requestBody = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            var conference = JsonConvert.DeserializeObject<Conference>(requestBody);

            log.LogInformation($"Conference #{conference.ConferenceId} announced for room: {conference.RoomName}");
            return conference;
        }
    }
}