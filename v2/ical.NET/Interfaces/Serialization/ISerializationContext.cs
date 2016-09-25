using ical.net.Interfaces.General;

namespace ical.net.Interfaces.Serialization
{
    public interface ISerializationContext : IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();
    }
}