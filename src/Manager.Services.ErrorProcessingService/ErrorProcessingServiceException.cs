using System;
using System.Runtime.Serialization;

namespace Manager.Services.ErrorProcessingService
{
    [Serializable]
    internal class ErrorProcessingServiceException : Exception
    {
        public ErrorProcessingServiceException()
        {
        }

        public ErrorProcessingServiceException(string message) : base(message)
        {
        }

        public ErrorProcessingServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ErrorProcessingServiceException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}