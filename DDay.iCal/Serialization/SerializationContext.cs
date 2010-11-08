using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal.Serialization
{
    public class SerializationContext :        
        ISerializationContext
    {
        #region Static Private Fields

        static private SerializationContext _Default;

        #endregion

        #region Static Public Properties

        /// <summary>
        /// Gets the Singleton instance of the SerializationContext class.
        /// </summary>
        static public ISerializationContext Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new SerializationContext();
                }

                // Create a new serialization context that doesn't contain any objects
                // (and is non-static).  That way, if any objects get pushed onto
                // the serialization stack when the Default serialization context is used,
                // and something goes wrong and the objects don't get popped off the stack,
                // we don't need to worry (as much) about a memory leak, because the
                // objects weren't pushed onto a stack referenced by a static variable.
                SerializationContext ctx = new SerializationContext();
                ctx.m_ServiceProvider = _Default.m_ServiceProvider;
                return ctx;
            }
        }

        #endregion

        #region Private Fields

        private Stack<WeakReference> m_Stack = new Stack<WeakReference>();
        private ServiceProvider m_ServiceProvider = new ServiceProvider();

        #endregion

        #region Constructors

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializationSettings());
            SetService(new SerializerFactory());
            SetService(new ComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new EncodingStack());
            SetService(new EncodingProvider(this));            
            SetService(new CompositeProcessor<IICalendar>());
            SetService(new CompositeProcessor<ICalendarComponent>());
            SetService(new CompositeProcessor<ICalendarProperty>());            
        }

        #endregion

        #region ISerializationContext Members

        virtual public void Push(object item)
        {
            if (item != null)
                m_Stack.Push(new WeakReference(item));
        }

        virtual public object Pop()
        {
            if (m_Stack.Count > 0)
                return m_Stack.Pop().Target;
            return null;
        }

        virtual public object Peek()
        {
            if (m_Stack.Count > 0)
                return m_Stack.Peek().Target;
            return null;
        }        

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return m_ServiceProvider.GetService(serviceType);
        }

        virtual public object GetService(string name)
        {
            return m_ServiceProvider.GetService(name);
        }

        virtual public T GetService<T>()
        {
            return m_ServiceProvider.GetService<T>();
        }

        virtual public T GetService<T>(string name)
        {
            return m_ServiceProvider.GetService<T>(name);
        }

        virtual public void SetService(string name, object obj)
        {
            m_ServiceProvider.SetService(name, obj);
        }

        virtual public void SetService(object obj)
        {
            m_ServiceProvider.SetService(obj);
        }

        virtual public void RemoveService(Type type)
        {
            m_ServiceProvider.RemoveService(type);
        }

        virtual public void RemoveService(string name)
        {
            m_ServiceProvider.RemoveService(name);
        }

        #endregion
    }
}
