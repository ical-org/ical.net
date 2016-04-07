using System;

namespace DDay.iCal.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, ISerializationContext ctx);
    }
}
