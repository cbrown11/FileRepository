using ChoETL;
using FileRepository.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileRepository.Convertors
{
    public static class ObjectToCsvStream
    {
        public static void Convert(JToken jObject, Stream mem, bool ignoresHeader)
        {
            var reader = new ChoJSONReader(jObject);
            try
            {
                using (var json = new ChoJSONReader(jObject)
                    .Configure(c => c.FlattenNode = true)
                    .JsonSerializationSettings(s => s.DateParseHandling = DateParseHandling.None)
                    )
                {

                    if (ignoresHeader)
                    {
                        using var w = new ChoCSVWriter(mem);
                        w.Write(json);
                    }
                    else
                    {
                        using var w = new ChoCSVWriter(mem).WithFirstLineHeader();
                        w.Write(json);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ConvertObjectToCsvStreamException("An error converting json object to csv stream", ex);
            }
        }
    }

}
