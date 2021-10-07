using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;

namespace MessageBusDemo.CommonHelpers
{
    public static class ServiceBusManager
    {

        public static async Task CreateDemoQueues()
        {
            var managementClient = new ManagementClient(Settings.ConnectionString);

            await CreateDemoQueue(managementClient);

            await CreateMockResourceQueue(managementClient);
        }

        /// <summary>
        /// Short DefaultMessageTimeToLive has heavy implications on dead-lettering and deferred messages retrieval
        /// Comment/uncomment the DefaultMessageTimeToLive line to demonstrate the implications
        /// </summary>
        /// <returns></returns>
        private static async Task CreateDemoQueue(ManagementClient managementClient)
        {
            if (!await managementClient.QueueExistsAsync(Settings.QueuePath))
            {
                var description = new QueueDescription(Settings.QueuePath)
                {
                    LockDuration = TimeSpan.FromSeconds(5),
                    MaxDeliveryCount = Settings.QueueMaxDeliveryCount,

                    // DefaultMessageTimeToLive = TimeSpan.FromSeconds(40),
                    EnableDeadLetteringOnMessageExpiration = true
                };

                await managementClient.CreateQueueAsync(description);
            }
        }

        public static async Task EnableMockResourceQueue()
        {
            var managementClient = new ManagementClient(Settings.ConnectionString);

            if (!await managementClient.QueueExistsAsync(Settings.ForwardingQueuePath))
            {
                await CreateMockResourceQueue(managementClient);
            }

            var mockResourceQueue = await managementClient.GetQueueAsync(Settings.ForwardingQueuePath);
            
            if (mockResourceQueue.Status != EntityStatus.Active)
            {
                mockResourceQueue.Status = EntityStatus.Active;
                await managementClient.UpdateQueueAsync(mockResourceQueue);
            }
        }

        public static async Task DisableMockResourceQueue()
        {
            var managementClient = new ManagementClient(Settings.ConnectionString);

            if (!await managementClient.QueueExistsAsync(Settings.ForwardingQueuePath))
            {
                await CreateMockResourceQueue(managementClient);
            }

            var mockResourceQueue = await managementClient.GetQueueAsync(Settings.ForwardingQueuePath);

            if (mockResourceQueue.Status != EntityStatus.Disabled)
            {
                mockResourceQueue.Status = EntityStatus.Disabled;
                await managementClient.UpdateQueueAsync(mockResourceQueue);
            }
        }

        private static async Task CreateMockResourceQueue(ManagementClient managementClient)
        {
            if (!await managementClient.QueueExistsAsync(Settings.ForwardingQueuePath))
            {
                await managementClient.CreateQueueAsync(Settings.ForwardingQueuePath);
            }
        }
    }
}