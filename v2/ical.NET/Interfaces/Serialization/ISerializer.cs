using System;
using System.IO;
using System.Text;
using IServiceProvider = ical.net.Interfaces.General.IServiceProvider;

namespace ical.net.Interfaces.Serialization
{
    public interface ISerializer : IServiceProvider
    {
        ISerializationContext SerializationContext { get; set; }

        Type TargetType { get; }
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}