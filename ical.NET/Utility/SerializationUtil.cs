using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ical.Net.Utility
{
    public class SerializationUtil
    {
        #region Static Public Methods

        public static object GetUninitializedObject(Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        public static void OnDeserializing(object obj)
        {
            var ctx = new StreamingContext(StreamingContextStates.All);
            foreach (var mi in GetDeserializingMethods(obj.GetType()))
            {
                mi.Invoke(obj, new object[] {ctx});
            }
        }

        public static void OnDeserialized(object obj)
        {
            var ctx = new StreamingContext(StreamingContextStates.All);
            foreach (var mi in GetDeserializedMethods(obj.GetType()))
            {
                mi.Invoke(obj, new object[] {ctx});
            }
        }

        #endregion

        #region Static Private Methods

        private static IEnumerable<MethodInfo> GetDeserializingMethods(Type targetType)
        {
            if (targetType != null)
            {
                // FIXME: cache this
                foreach (var mi in targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attrs = mi.GetCustomAttributes(typeof (OnDeserializingAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        yield return mi;
                    }
                }
            }
        }

        private static IEnumerable<MethodInfo> GetDeserializedMethods(Type targetType)
        {
            if (targetType != null)
            {
                // FIXME: cache this
                foreach (var mi in targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attrs = mi.GetCustomAttributes(typeof (OnDeserializedAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        yield return mi;
                    }
                }
            }
        }

        #endregion
    }
}