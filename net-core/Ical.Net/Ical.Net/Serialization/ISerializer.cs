using System;
using System.IO;
using System.Text;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Serialization
{
    public interface ISerializer : IServiceProvider
    {
        SerializationContext SerializationContext { get; set; }

        Type TargetType { get; }
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}