using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;

namespace DDay.iCal.Serialization
{    
    public interface ISerializable
    {
        ISerializationContext SerializationContext { get; set; }

        string SerializeToString(ICalendarObject obj);
        void Serialize(ICalendarObject obj, Stream stream, Encoding encoding);
        object Deserialize(TextReader tr, Type iCalendarType);
        object Deserialize(Stream stream, Encoding encoding, Type iCalendarType);
    }
}
