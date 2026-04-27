namespace FileRepository
{
    public class FileProperties
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Modified { get; set; }
    }
}
