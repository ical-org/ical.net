using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Ical.Net.Serialization
{
    public class GenericListSerializer : SerializerBase
    {
        readonly Type _innerType;
        readonly Type _objectType;

        public GenericListSerializer(Type objectType)
        {
            _innerType = objectType.GetGenericArguments()[0];

            var listDef = typeof (List<>);
            _objectType = listDef.MakeGenericType(typeof (object));
        }

        public override Type TargetType => _objectType;

        public override string SerializeToString(object obj) => throw new NotImplementedException();

        MethodInfo _addMethodInfo;
        public override object Deserialize(TextReader tr)
        {
            if (!(SerializationContext.Peek() is ICalendarProperty p))
            {
                return null;
            }

            // Get a serializer factory to deserialize the contents of this list
            var listObj = Activator.CreateInstance(_objectType);
            if (listObj == null)
            {
                return null;
            }

            // Get a serializer for the inner type
            var sf = GetService<ISerializerFactory>();
            if (!(sf.Build(_innerType, SerializationContext) is IStringSerializer stringSerializer))
            {
                return null;
            }
            // Deserialize the inner object
            var value = tr.ReadToEnd();

            // If deserialization failed, pass the string value into the list.
            var objToAdd = stringSerializer.Deserialize(new StringReader(value)) ?? value;

            // FIXME: cache this
            if (_addMethodInfo == null)
            {
                _addMethodInfo = _objectType.GetMethod("Add");
            }

            // Determine if the returned object is an IList<ObjectType>, rather than just an ObjectType.
            if (objToAdd is IList add)
            {
                //Deserialization returned an IList<ObjectType>, instead of an ObjectType.  So enumerate through the items in the list and add
                //them individually to our list.
                foreach (var innerObj in add)
                {
                    _addMethodInfo.Invoke(listObj, new[] {innerObj});
                }
            }
            else
            {
                // Add the object to the list
                _addMethodInfo.Invoke(listObj, new[] {objToAdd});
            }
            return listObj;
        }
    }
}