//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Ical.Net.Serialization;

public class GenericListSerializer : SerializerBase
{
    private readonly Type _innerType;

    public GenericListSerializer(Type objectType)
    {
        _innerType = objectType.GetGenericArguments()[0];
    }

    public override Type TargetType => typeof(List<object>);

    public override string SerializeToString(object? obj) => throw new NotImplementedException();

    public override object? Deserialize(TextReader tr)
    {
        var p = SerializationContext.Peek() as ICalendarProperty;
        if (p == null)
        {
            return null;
        }

        var listObj = new List<object>();

        // Get a serializer for the inner type
        var sf = GetService<ISerializerFactory>();
        var stringSerializer = sf.Build(_innerType, SerializationContext) as IStringSerializer;
        if (stringSerializer == null)
        {
            return null;
        }
        // Deserialize the inner object
        var value = tr.ReadToEnd();

        // If deserialization failed, pass the string value into the list.
        var objToAdd = stringSerializer.Deserialize(new StringReader(value)) ?? value;

        // Determine if the returned object is an IList<ObjectType>, rather than just an ObjectType.
        if (objToAdd is IList add)
        {
            //Deserialization returned an IList<ObjectType>, instead of an ObjectType.  So enumerate through the items in the list and add
            //them individually to our list.
            foreach (var innerObj in add)
            {
                listObj.Add(innerObj);
            }
        }
        else
        {
            // Add the object to the list
            listObj.Add(objToAdd);
        }
        return listObj;
    }
}
