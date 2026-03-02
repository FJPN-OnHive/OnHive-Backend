using System.Runtime.Serialization;

namespace EHive.Payments.Domain.Exceptions
{
    [Serializable]
    public class InvalidPaymentException : Exception
    {
        public InvalidPaymentException(string? message) : base(message)
        {
        }

        protected InvalidPaymentException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}