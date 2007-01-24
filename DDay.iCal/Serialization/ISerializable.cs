using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public interface ISerializable
    {
        string SerializeToString();
        void Serialize(Stream stream, Encoding encoding);        
    }
}
