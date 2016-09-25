using System.Text;

namespace ical.net.Interfaces.Serialization
{
    public interface IEncodingStack
    {
        Encoding Current { get; }
        void Push(Encoding encoding);
        Encoding Pop();
    }
}