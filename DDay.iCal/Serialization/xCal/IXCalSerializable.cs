using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using System.Xml;

namespace DDay.iCal.Serialization.xCal
{
    public interface IXCalSerializable : ISerializable
    {
        void Serialize(XmlTextWriter xtw);
        iCalObject Deserialize(XmlTextReader xtr, Type iCalendarType);
    }
}
