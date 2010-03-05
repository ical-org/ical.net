using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;

namespace DDay.iCal.Serialization
{    
    public interface ISerializer
    {
        ISerializationContext SerializationContext { get; set; }
        T GetService<T>();

        string SerializeToString(object obj);
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(TextReader tr);
        object Deserialize(Stream stream, Encoding encoding);
    }
}
