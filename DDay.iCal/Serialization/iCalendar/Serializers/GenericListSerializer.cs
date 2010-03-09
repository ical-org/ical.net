using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace DDay.iCal.Serialization.iCalendar
{
    public class GenericListSerializer :
        SerializerBase
    {
        #region Private Fields

        Type _InnerType;
        Type _ObjectType;

        #endregion

        #region Constructors

        public GenericListSerializer(Type objectType)
        {
            _InnerType = objectType.GetGenericArguments()[0];
            
            Type listDef = typeof(List<>);
            _ObjectType = listDef.MakeGenericType(_InnerType);
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return _ObjectType; }
        }

        public override string SerializeToString(object obj)
        {
            // NOTE: this behavior is implemented in the PropertySerializer.
            throw new NotImplementedException();
        }

        public override object Deserialize(TextReader tr)
        {
            ICalendarProperty p = SerializationContext.Peek() as ICalendarProperty;
            if (p != null)
            {
                // Get a serializer factory to deserialize the contents of this list
                ISerializerFactory sf = GetService<ISerializerFactory>();

                object listObj = Activator.CreateInstance(_ObjectType);
                if (listObj != null)
                {
                    IStringSerializer stringSerializer = sf.Build(_InnerType, SerializationContext) as IStringSerializer;
                    if (stringSerializer != null)
                    {
                        // Deserialize the inner object
                        object objToAdd = stringSerializer.Deserialize(tr);

                        // Ensure the resulting object can be added to the list.
                        if (objToAdd != null && _InnerType.IsAssignableFrom(objToAdd.GetType()))
                        {
                            // FIXME: cache this
                            MethodInfo mi = _ObjectType.GetMethod("Add");
                            if (mi != null)
                            {
                                // Add the object to the list
                                mi.Invoke(listObj, new object[] { objToAdd });

                                return listObj;
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
