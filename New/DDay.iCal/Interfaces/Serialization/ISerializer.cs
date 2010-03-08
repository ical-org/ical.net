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
        T GetService<T>(string name);

        Type TargetType { get; }        
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}
