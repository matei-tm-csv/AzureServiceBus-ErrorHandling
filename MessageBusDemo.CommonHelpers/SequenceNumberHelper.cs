namespace MessageBusDemo.CommonHelpers
{
    public static class SequenceNumberHelper
    {
        public static long Decode(string body)
        {
            return int.Parse(body.Split(' ')[1]);
        }

        public static string Encode(string sequenceId)
        {
            return $"Deferred {sequenceId} message addicts have arrived";
        }
    }
}