using ChoETL;
using FileRepository.Exceptions;

namespace FileRepository.Convertors
{
    public static class CsvStreamToList
    {
        public static IList<T> Convert<T>(Stream mem, bool ignoresHeader = true, string delimiter=",", bool throwAndStopOnMissingField=true) where T : class
        {
            try
            {
                using var reader = new ChoCSVReader<T>(mem)
                    .WithFirstLineHeader(ignoresHeader)
                    .WithDelimiter(delimiter)
                    .ThrowAndStopOnMissingField(throwAndStopOnMissingField)
                    ;
                return reader.ToList();
            }
            catch (Exception ex)
            {
                throw new ConvertObjectToCsvStreamException("An error converting csv stream to List", ex);
            }
        }

        public static void Validate<T>(Stream mem, bool ignoresHeader = true, string delimiter = ",", bool throwAndStopOnMissingField = true) where T : class
        {

                using var reader = new ChoCSVReader<T>(mem)
                    .WithFirstLineHeader(ignoresHeader)
                    .WithDelimiter(delimiter)
                    .ThrowAndStopOnMissingField(throwAndStopOnMissingField)
                    ;
                {
                    reader.Validate();
                }

        }

        public static bool IsValid<T>(Stream mem, out Exception aggEx, bool ignoresHeader = true, string delimiter = ",", bool throwAndStopOnMissingField = true) where T : class
        {
            using var reader = new ChoCSVReader<T>(mem)
                .WithFirstLineHeader(ignoresHeader)
                .WithDelimiter(delimiter);
            {
                var result =  reader.IsValid(out aggEx);
                return result;
            }

        }
    }
}
