using System;

namespace MessageBusDemo.AzureFunctionsReceiver
{
    public class PoisonException : Exception
    {
        public PoisonException() : base()
        {
        }

        public PoisonException(string message) : base(message)
        {
        }

        public PoisonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}