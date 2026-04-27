namespace FileRepository.AzureStorage.Exceptions
{
    [Serializable]
    public class BlobClientRetrievalException : Exception
    {
        public BlobClientRetrievalException()
        {
        }

        public BlobClientRetrievalException(string message)
            : base(message)
        {
        }

        public BlobClientRetrievalException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BlobClientRetrievalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
