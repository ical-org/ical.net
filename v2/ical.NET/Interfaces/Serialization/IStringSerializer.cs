using System.IO;

namespace ical.net.Interfaces.Serialization
{
    public interface IStringSerializer : ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}