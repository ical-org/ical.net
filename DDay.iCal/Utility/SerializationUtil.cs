using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace DDay.iCal
{
    public class SerializationUtil
    {
        #region Static Public Methods

        static public object GetUninitializedObject(Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        static public void OnDeserializing(object obj)
        {
            StreamingContext ctx = new StreamingContext(StreamingContextStates.All);
            foreach (MethodInfo mi in GetDeserializingMethods(obj.GetType()))
                mi.Invoke(obj, new object[] { ctx });
        }

        static public void OnDeserialized(object obj)
        {
            StreamingContext ctx = new StreamingContext(StreamingContextStates.All);
            foreach (MethodInfo mi in GetDeserializedMethods(obj.GetType()))
                mi.Invoke(obj, new object[] { ctx });
        } 

        #endregion

        #region Static Private Methods

        static private IEnumerable<MethodInfo> GetDeserializingMethods(Type targetType)
        {
            if (targetType != null)
            {
                // FIXME: cache this
                foreach (MethodInfo mi in targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    object[] attrs = mi.GetCustomAttributes(typeof(OnDeserializingAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                        yield return mi;
                }
            }
        }

        static private IEnumerable<MethodInfo> GetDeserializedMethods(Type targetType)
        {
            if (targetType != null)
            {
                // FIXME: cache this
                foreach (MethodInfo mi in targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    object[] attrs = mi.GetCustomAttributes(typeof(OnDeserializedAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                        yield return mi;
                }
            }
        } 

        #endregion
    }
}
