using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization
{    
    public interface ISerializable
    {
        ISerializationContext SerializationContext { get; set; }

        string SerializeToString();
        void Serialize(Stream stream, Encoding encoding);
        iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType);
    }
}
