namespace FileRepository.Sftp.Exceptions
{
    [Serializable]
    public class SftpException : Exception
    {
        public SftpException()
        {
        }

        public SftpException(string message)
            : base(message)
        {
        }

        public SftpException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SftpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
