using System;
using System.IO;
using System.Text;

namespace Ical.Net.Serialization
{
    public abstract class SerializerBase : IStringSerializer
    {
        protected SerializerBase()
        {
            SerializationContext = SerializationContext.Default;
        }

        protected SerializerBase(SerializationContext ctx)
        {
            SerializationContext = ctx;
        }

        public SerializationContext SerializationContext { get; set; }

        public abstract Type TargetType { get; }

        /// <summary> Converts <paramref name="obj"/> into the vCard Value Format </summary>
        /// <returns>null when <paramref name="obj"/> is null or not of the proper Type</returns>
        public abstract string SerializeToString(object obj);
        public abstract object? Deserialize(TextReader tr);

        public object? Deserialize(Stream stream, Encoding encoding)
        {
            using var sr = new StreamReader(stream, encoding);
            var encodingStack = GetService<EncodingStack>();
            encodingStack.Push(encoding);
            var obj = Deserialize(sr);
            encodingStack.Pop();
            return obj;
        }

        public void Serialize(object obj, Stream stream, Encoding encoding)
        {
            // NOTE: we don't use a 'using' statement here because
            // we don't want the stream to be closed by this serialization.
            // Fixes bug #3177278 - Serialize closes stream

            const int defaultBuffer = 1024;     //This is StreamWriter's built-in default buffer size
            using var sw = new StreamWriter(stream, encoding, defaultBuffer, leaveOpen: true);
            // Push the current object onto the serialization stack
            SerializationContext.Push(obj);

            // Push the current encoding on the stack
            var encodingStack = GetService<EncodingStack>();
            encodingStack.Push(encoding);

            sw.Write(SerializeToString(obj));

            // Pop the current encoding off the serialization stack
            encodingStack.Pop();

            // Pop the current object off the serialization stack
            SerializationContext.Pop();
        }

        public object GetService(Type serviceType) => SerializationContext.GetService(serviceType);

        public object GetService(string name) => SerializationContext.GetService(name);

        public T GetService<T>() => SerializationContext.GetService<T>();

        public T GetService<T>(string name) => SerializationContext.GetService<T>(name);

        public void SetService(string name, object obj) => SerializationContext.SetService(name, obj);

        public void SetService(object obj) => SerializationContext.SetService(obj);

        public void RemoveService(Type type) => SerializationContext.RemoveService(type);

        public void RemoveService(string name) => SerializationContext.RemoveService(name);
    }
}