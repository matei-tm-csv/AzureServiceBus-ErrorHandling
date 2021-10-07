namespace MessageBusDemo.CommonHelpers
{
    public static class Settings
    {
        //ToDo: Enter a valid Service Bus connection string
        public static string ConnectionString = "Endpoint=sb://yourdemoservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=**yourdemoapikey**";
        public static string QueuePath = "errorhandlingdemo";
        public static string TopicPath = QueuePath + "-topic";
        public static string ForwardingQueuePath = "errorhandlingdemoforwarding";
        public const int QueueMaxDeliveryCount = 6; // a custom value for demo purposes
        public const int MaxProcessingCount = QueueMaxDeliveryCount - 2; // a custom value lower than MaxDeliveryCount
    }
}
