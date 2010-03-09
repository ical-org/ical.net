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

        static private ISerializationContext _Default;

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
                    _Default = new SerializationContext();
                return _Default;
            }
        }

        #endregion

        #region Private Fields

        private IDictionary<Type, object> m_TypedServices = new Dictionary<Type, object>();
        private IDictionary<string, object> m_NamedServices = new Dictionary<string, object>();
        private Stack<object> m_Stack = new Stack<object>();

        #endregion

        #region Constructors

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializationSettings());
            SetService(new SerializerFactory());
            SetService(new ComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new CompositeProcessor<IICalendar>());
            SetService(new CompositeProcessor<ICalendarComponent>());
            SetService(new CompositeProcessor<ICalendarProperty>());
        }

        #endregion

        #region ISerializationContext Members

        virtual public void Push(object item)
        {
            if (item != null)
                m_Stack.Push(item);
        }

        virtual public object Pop()
        {
            if (m_Stack.Count > 0)
                return m_Stack.Pop();
            return null;
        }

        virtual public object Peek()
        {
            if (m_Stack.Count > 0)
                return m_Stack.Peek();
            return null;
        }

        virtual public object GetService(string name)
        {
            if (m_NamedServices.ContainsKey(name))
                return m_NamedServices[name];
            return null;
        }

        virtual public void SetService(string name, object obj)
        {
            if (!string.IsNullOrEmpty(name) && obj != null)
                m_NamedServices[name] = obj;
        }

        virtual public void SetService(object obj)
        {
            if (obj != null)
            {
                Type type = obj.GetType();
                m_TypedServices[type] = obj;

                // Get interfaces for the given type
                foreach (Type iface in type.GetInterfaces())
                    m_TypedServices[iface] = obj;
            }
        }

        virtual public void RemoveService(Type type)
        {
            if (type != null)
            {
                if (m_TypedServices.ContainsKey(type))
                     m_TypedServices.Remove(type);

                // Get interfaces for the given type
                foreach (Type iface in type.GetInterfaces())
                {
                    if (m_TypedServices.ContainsKey(iface))
                     m_TypedServices.Remove(iface);
                }
            }
        }

        virtual public void RemoveService(string name)
        {
            if (m_NamedServices.ContainsKey(name))
                m_NamedServices.Remove(name);
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            if (m_TypedServices.ContainsKey(serviceType))
                return m_TypedServices[serviceType];
            return null;
        }

        #endregion    
    }
}
