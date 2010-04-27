using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace DDay.iCal.Serialization.iCalendar
{
    public abstract class SerializerBase :
        IStringSerializer
    {
        #region Private Fields

        ISerializationContext m_SerializationContext;

        #endregion

        #region Constructors

        public SerializerBase()
        {
            m_SerializationContext = DDay.iCal.Serialization.SerializationContext.Default;
        }

        public SerializerBase(ISerializationContext ctx)
        {
            m_SerializationContext = ctx;
        }

        #endregion

        #region ISerializer Members

        virtual public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }        

        public abstract Type TargetType { get; }
        public abstract string SerializeToString(object obj);
        public abstract object Deserialize(TextReader tr);

        public object Deserialize(Stream stream, Encoding encoding)
        {
            object obj = null;
            using (StreamReader sr = new StreamReader(stream, encoding))
            {
                // Push the current encoding on the stack
                IEncodingStack encodingStack = GetService<IEncodingStack>();
                encodingStack.Push(encoding);

                obj = Deserialize(sr);

                // Pop the current encoding off the stack
                encodingStack.Pop();
            }
            return obj;
        }

        public void Serialize(object obj, Stream stream, Encoding encoding)
        {
            using (StreamWriter sw = new StreamWriter(stream, encoding))
            {
                // Push the current object onto the serialization stack
                SerializationContext.Push(obj);

                // Push the current encoding on the stack
                IEncodingStack encodingStack = GetService<IEncodingStack>();
                encodingStack.Push(encoding);

                sw.Write(SerializeToString(obj));

                // Pop the current encoding off the serialization stack
                encodingStack.Pop();

                // Pop the current object off the serialization stack
                SerializationContext.Pop();
            }
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            if (SerializationContext != null)
                return SerializationContext.GetService(serviceType);
            return null;
        }

        virtual public object GetService(string name)
        {
            if (SerializationContext != null)
                return SerializationContext.GetService(name);
            return null;
        }

        virtual public T GetService<T>()
        {
            if (SerializationContext != null)
                return SerializationContext.GetService<T>();
            return default(T);
        }

        virtual public T GetService<T>(string name)
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
