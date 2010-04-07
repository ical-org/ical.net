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

        virtual public T GetService<T>()
        {
            if (SerializationContext != null)
            {
                object obj = SerializationContext.GetService(typeof(T));
                if (obj != null)
                    return (T)obj;
            }
            return default(T);
        }

        virtual public T GetService<T>(string name)
        {
            if (SerializationContext != null)
            {
                object obj = SerializationContext.GetService(name);
                if (obj != null)
                    return (T)obj;
            }
            return default(T);
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
    }
}
