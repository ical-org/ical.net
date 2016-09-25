using System;

namespace ical.net.Interfaces.Serialization.Factory
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, ISerializationContext ctx);
    }
}