using System.IO;

namespace DDay.iCal.Serialization
{
    public interface IStringSerializer :
        ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}
