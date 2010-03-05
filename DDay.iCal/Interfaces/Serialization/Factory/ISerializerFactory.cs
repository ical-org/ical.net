using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Build(Type objectType, ISerializationContext ctx);
    }
}
