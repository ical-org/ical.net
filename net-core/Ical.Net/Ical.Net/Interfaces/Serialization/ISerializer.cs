using System;
using System.IO;
using System.Text;
using Ical.Net.Serialization;
using IServiceProvider = Ical.Net.Interfaces.General.IServiceProvider;

namespace Ical.Net.Interfaces.Serialization
{
    public interface ISerializer : IServiceProvider
    {
        SerializationContext SerializationContext { get; set; }

        Type TargetType { get; }
        void Serialize(object obj, Stream stream, Encoding encoding);
        object Deserialize(Stream stream, Encoding encoding);
    }
}