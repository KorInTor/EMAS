using System.Runtime.Serialization;

namespace EMAS.Exceptions
{
    [Serializable]
    public class UnknownEventTypeException : Exception
    {
        public UnknownEventTypeException()
        {
        }

        public UnknownEventTypeException(string? message) : base(message)
        {
        }

        public UnknownEventTypeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnknownEventTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}