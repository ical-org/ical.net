using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Ical.Net.Utility
{
    public class SerializationUtil
    {
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

        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static IEnumerable<MethodInfo> GetDeserializingMethods(Type targetType)
        {
            if (targetType == null)
            {
                yield break;
            }

            // FIXME: cache this
            foreach (var methodInfo in from targetTypeMethodInfo in targetType.GetMethods(_bindingFlags)
                     let attrs = targetTypeMethodInfo.GetCustomAttributes(typeof (OnDeserializingAttribute), false)
                     where attrs.Length > 0
                     select targetTypeMethodInfo)
            {
                yield return methodInfo;
            }
        }

        private static IEnumerable<MethodInfo> GetDeserializedMethods(Type targetType)
        {
            if (targetType == null)
            {
                yield break;
            }

            // FIXME: cache this
            foreach (var methodInfo in from targetTypeMethodInfo in targetType.GetMethods(_bindingFlags)
                let attrs = targetTypeMethodInfo.GetCustomAttributes(typeof (OnDeserializedAttribute), true)
                where attrs.Length > 0
                select targetTypeMethodInfo)
            {
                yield return methodInfo;
            }
        }
    }
}