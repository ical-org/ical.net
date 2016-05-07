using System;
using System.IO;
using System.Text;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public abstract class SerializerBase :
        IStringSerializer
    {
        #region Private Fields

        ISerializationContext _mSerializationContext;

        #endregion

        #region Constructors

        public SerializerBase()
        {
            _mSerializationContext = Serialization.SerializationContext.Default;
        }

        public SerializerBase(ISerializationContext ctx)
        {
            _mSerializationContext = ctx;
        }

        #endregion

        #region ISerializer Members

        public virtual ISerializationContext SerializationContext
        {
            get { return _mSerializationContext; }
            set { _mSerializationContext = value; }
        }        

        public abstract Type TargetType { get; }
        public abstract string SerializeToString(object obj);
        public abstract object Deserialize(TextReader tr);

        public object Deserialize(Stream stream, Encoding encoding)
        {
            object obj = null;
            using (var sr = new StreamReader(stream, encoding))
            {
                // Push the current encoding on the stack
                var encodingStack = GetService<IEncodingStack>();
                encodingStack.Push(encoding);

                obj = Deserialize(sr);

                // Pop the current encoding off the stack
                encodingStack.Pop();
            }
            return obj;
        }

        public void Serialize(object obj, Stream stream, Encoding encoding)
        {
            // NOTE: we don't use a 'using' statement here because
            // we don't want the stream to be closed by this serialization.
            // Fixes bug #3177278 - Serialize closes stream
            var sw = new StreamWriter(stream, encoding);
            
            // Push the current object onto the serialization stack
            SerializationContext.Push(obj);

            // Push the current encoding on the stack
            var encodingStack = GetService<IEncodingStack>();
            encodingStack.Push(encoding);

            sw.Write(SerializeToString(obj));

            // Pop the current encoding off the serialization stack
            encodingStack.Pop();

            // Pop the current object off the serialization stack
            SerializationContext.Pop();
        }

        #endregion

        #region IServiceProvider Members

        public virtual object GetService(Type serviceType)
        {
            if (SerializationContext != null)
                return SerializationContext.GetService(serviceType);
            return null;
        }

        public virtual object GetService(string name)
        {
            if (SerializationContext != null)
                return SerializationContext.GetService(name);
            return null;
        }

        public virtual T GetService<T>()
        {
            if (SerializationContext != null)
                return SerializationContext.GetService<T>();
            return default(T);
        }

        public virtual T GetService<T>(string name)
        {
            if (SerializationContext != null)
                return SerializationContext.GetService<T>(name);
            return default(T);
        }

        public void SetService(string name, object obj)
        {
            if (SerializationContext != null)
                SerializationContext.SetService(name, obj);
        }

        public void SetService(object obj)
        {
            if (SerializationContext != null)
                SerializationContext.SetService(obj);
        }

        public void RemoveService(Type type)
        {
            if (SerializationContext != null)
                SerializationContext.RemoveService(type);
        }

        public void RemoveService(string name)
        {
            if (SerializationContext != null)
                SerializationContext.RemoveService(name);
        }

        #endregion
    }
}
