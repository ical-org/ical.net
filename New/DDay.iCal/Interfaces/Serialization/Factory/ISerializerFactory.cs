using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    public interface ISerializerFactory
    {
        ISerializer Create(ICalendarObject obj, ISerializationContext ctx);
    }
}
