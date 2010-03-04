using System;
using System.Collections.Generic;
using System.Text;

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

        private IDictionary<Type, object> m_Services = new Dictionary<Type, object>();

        #endregion

        #region Constructors

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializationSettings());
            SetService(new ComponentFactory());
        }

        #endregion

        #region ISerializationContext Members

        virtual public void SetService(object obj)
        {
            if (obj != null)
            {
                Type type = obj.GetType();
                m_Services[type] = obj;

                // Get interfaces for the given type
                foreach (Type iface in type.GetInterfaces())
                    m_Services[iface] = obj;
            }
        }

        virtual public void RemoveService(Type type)
        {
            if (type != null)
            {
                if (m_Services.ContainsKey(type))
                     m_Services.Remove(type);

                // Get interfaces for the given type
                foreach (Type iface in type.GetInterfaces())
                {
                    if (m_Services.ContainsKey(iface))
                     m_Services.Remove(iface);
                }
            }
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            if (m_Services.ContainsKey(serviceType))
                return m_Services[serviceType];
            return null;
        }

        #endregion       
    }
}
