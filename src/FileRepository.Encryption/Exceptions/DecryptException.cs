namespace FileRepository.Encryption.Exceptions
{
    [Serializable]
    public class DecryptException : Exception
    {
        public DecryptException()
        {
        }

        public DecryptException(string message)
            : base(message)
        {
        }

        public DecryptException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DecryptException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
