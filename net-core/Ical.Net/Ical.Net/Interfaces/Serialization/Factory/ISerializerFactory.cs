using System;
using Ical.Net.Serialization;

namespace Ical.Net.Interfaces.Serialization.Factory
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, SerializationContext ctx);
    }
}