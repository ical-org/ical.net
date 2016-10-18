using System;
using ical.net.Serialization;

namespace ical.net.Interfaces.Serialization.Factory
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, SerializationContext ctx);
    }
}