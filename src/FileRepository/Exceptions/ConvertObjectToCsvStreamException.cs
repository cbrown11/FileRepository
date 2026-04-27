namespace FileRepository.Exceptions
{
    public class ConvertObjectToCsvStreamException : Exception
    {
        public ConvertObjectToCsvStreamException(string message)
            : base(message) { }

        public ConvertObjectToCsvStreamException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}