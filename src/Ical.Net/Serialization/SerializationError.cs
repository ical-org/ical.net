namespace Ical.Net.Serialization
{
    public class SerializationError
    {
        public string PropertyName { get; }
        public string ContentLine { get; }

        public SerializationError(string propertyName, string contentLine)
        {
            PropertyName = propertyName;
            ContentLine = contentLine;
        }
    }
}
