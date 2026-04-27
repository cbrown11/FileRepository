namespace FileRepository.Encryption.Exceptions
{
    [Serializable]
    public class EncryptException : Exception
    {
        public EncryptException()
        {
        }

        public EncryptException(string message)
            : base(message)
        {
        }

        public EncryptException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EncryptException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
