namespace FileRepository.AzureStorage.Exceptions
{
    [Serializable]
    public class FileExistsException : Exception
    {
        public FileExistsException()
        {
        }

        public FileExistsException(string message)
            : base(message)
        {
        }

        public FileExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FileExistsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
