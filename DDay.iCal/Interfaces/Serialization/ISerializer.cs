using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{    
    public interface ISerializer :
        IServiceProvider
    {
        ISerializationContext SerializationContext { get; set; }        

        Type TargetType { get; }        
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}
