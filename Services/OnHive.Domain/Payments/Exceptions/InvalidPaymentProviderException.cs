using System.Runtime.Serialization;

namespace EHive.Payments.Domain.Exceptions
{
    [Serializable]
    public class InvalidPaymentProviderException : Exception
    {
        public InvalidPaymentProviderException(string? message) : base(message)
        {
        }

        protected InvalidPaymentProviderException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}