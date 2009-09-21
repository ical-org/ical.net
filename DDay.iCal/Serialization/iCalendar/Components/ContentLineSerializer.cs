using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;
using System.Reflection;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class ContentLineSerializer : ISerializable
    {
        #region Static Public Methods

        /// <summary>
        /// For iCalendar components, automatically finds and retrieves fields that
        /// match the field specified in the <see cref="ContentLine"/>, and sets
        /// their value.
        /// <example>
        /// For example, if a public DTStart field exists in the specified component,
        /// (i.e. <c>public iCalDateTime DTStart;</c>)
        /// and a content line of <c>DTSTART;TZID=US-Eastern:20060830T090000</c> is
        /// encountered, this method will automatically set the value of the
        /// DTStart field to Aug. 30, 2006, 9:00 AM in the US-Eastern TimeZone.
        /// </example>
        /// <note>
        ///     It should not be necessary to invoke this method manually as it
        ///     is handled automatically during the iCalendar parsing.
        /// </note>
        /// </summary>
        /// <param name="cl">The <see cref="ContentLine"/> to process.</param>
        /// <param name="obj">The <see cref="iCalObject"/> to assign information to.</param>
        static public void DeserializeToObject(ContentLine cl, iCalObject obj)
        {
            if (cl.Name != null)
            {
                // Set the parent of the ContentLine to the object
                cl.Parent = obj;

                string name = cl.Name;
                Type type = obj.GetType();

                // Remove X- from the property name, since
                // non-standard properties are named like
                // everything else, but also have the NonstandardAttribute
                // attached.
                if (name.StartsWith("X-"))
                    name = name.Remove(0, 2);

                // Replace invalid characters
                name = name.Replace("-", "_");

                //
                // Find the public field that matches the name of our content line (ignoring case)
                //
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Static);
                PropertyInfo property = null;

                if (field == null)
                    property = type.GetProperty(name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Static);

                if (field != null ||
                    property != null)
                {
                    // This is a non-standard property.  Let's load it into memory,
                    // So we can serialize it later
                    Property p = new Property(cl);
                    p.AddToParent();

                    // Get the field/property's value
                    object value = field == null ? property.GetValue(obj, null) : field.GetValue(obj);
                    Type itemType = field == null ? property.PropertyType : field.FieldType;
                    object[] itemAttributes = field == null ? property.GetCustomAttributes(true) : field.GetCustomAttributes(true);

                    Type elementType = itemType.IsArray ? itemType.GetElementType() : itemType;

                    // If it's an iCalDataType, or an array of iCalDataType, then let's fill it!
                    if (itemType.IsSubclassOf(typeof(iCalDataType)) ||
                        (itemType.IsArray && itemType.GetElementType().IsSubclassOf(typeof(iCalDataType))))
                    {
                        iCalDataType icdt = null;
                        if (!itemType.IsArray)
                            icdt = (iCalDataType)value;
                        if (icdt == null)
                            icdt = (iCalDataType)Activator.CreateInstance(elementType);

                        // Assign custom attributes for the specific field
                        icdt.Attributes = itemAttributes;

                        // Set the content line for the object.                        
                        icdt.ContentLine = cl;
                        
                        // It's an array, let's add an item to the end
                        if (itemType.IsArray)
                        {
                            ArrayList arr = new ArrayList();
                            if (value != null)
                                arr.AddRange((ICollection)value);
                            arr.Add(icdt);
                            if (field != null)
                                field.SetValue(obj, arr.ToArray(elementType));
                            else
                                property.SetValue(obj, arr.ToArray(elementType), null);
                        }
                        // Otherwise, set the value directly!
                        else
                        {
                            if (field != null)
                                field.SetValue(obj, icdt);
                            else property.SetValue(obj, icdt, null);
                        }
                    }
                    else
                    {
                        FieldInfo minValue = itemType.GetField("MinValue");
                        object minVal = (minValue != null) ? minValue.GetValue(null) : null;

                        if (itemType.IsArray)
                        {
                            ArrayList arr = new ArrayList();
                            if (value != null)
                                arr.AddRange((ICollection)value);
                            arr.Add(cl.Value);

                            if (field != null)
                                field.SetValue(obj, arr.ToArray(elementType));
                            else property.SetValue(obj, arr.ToArray(elementType), null);
                        }
                        // Always assign enum values
                        else if (itemType.IsEnum)
                        {
                            if (field != null)
                                field.SetValue(obj, Enum.Parse(itemType, cl.Value.Replace("-", "_"), true));
                            else property.SetValue(obj, Enum.Parse(itemType, cl.Value.Replace("-", "_"), true), null);
                        }
                        // Otherwise, set the value directly!
                        else if (value == null || value.Equals(minVal))
                        {
                            if (field != null)
                                field.SetValue(obj, cl.Value);
                            else property.SetValue(obj, cl.Value, null);
                        }
                        else ;// FIXME: throw single-value exception, if "strict" parsing is enabled
                    }
                }              
            }
        }

        #endregion

        #region Private Fields

        private string m_text;

        #endregion

        #region Constructors

        public ContentLineSerializer(string s)            
        {
            this.m_text = s;
        }

        #endregion

        #region ISerializable Members

        public string SerializeToString()
        {   
            // NOTE: Made this method more efficient by removing
            // the use of strings, and only using StringBuilders.
            // Also, the "while" loop was removed, and StringBuilder
            // modifications are kept at a minimum.
            StringBuilder result = new StringBuilder();
            StringBuilder current = new StringBuilder(m_text);

            // Wrap lines at 75 characters, per RFC 2445 "folding" technique
            int i = 0;
            if (current.Length > 75)
            {
                result.Append(current.ToString(0, 75) + "\r\n ");
                for (i = 75; i < current.Length - 74; i += 74)
                    result.Append(current.ToString(i, 74) + "\r\n ");
            }
            result.Append(current.ToString(i, current.Length - i));
            result.Append("\r\n");

            return result.ToString();
        }

        public void Serialize(Stream stream, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(SerializeToString());
            if (data.Length > 0)
                stream.Write(data, 0, data.Length);
        }

        public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }        

        #endregion        
    }
}
