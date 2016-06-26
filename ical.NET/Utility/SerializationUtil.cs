using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static Dictionary<Type, List<MethodInfo>> _onDeserializingMethods = new Dictionary<Type, List<MethodInfo>>(16);
        private static List<MethodInfo> GetDeserializingMethods(Type targetType)
        {
            if (targetType == null)
            {
                return new List<MethodInfo>();
            }

            if (_onDeserializingMethods.ContainsKey(targetType))
            {
                return _onDeserializingMethods[targetType];
            }

            var methodInfo = targetType.GetMethods(_bindingFlags)
                .Select(targetTypeMethodInfo => new
                {
                    targetTypeMethodInfo,
                    attrs = targetTypeMethodInfo.GetCustomAttributes(typeof(OnDeserializingAttribute), false)
                })
                .Where(t => t.attrs.Length > 0)
                .Select(t => t.targetTypeMethodInfo)
                .ToList();

            _onDeserializingMethods.Add(targetType, methodInfo);
            return methodInfo;
        }

        private static Dictionary<Type, List<MethodInfo>> _onDeserializedMethods = new Dictionary<Type, List<MethodInfo>>(16);
        private static List<MethodInfo> GetDeserializedMethods(Type targetType)
        {
            if (targetType == null)
            {
                return new List<MethodInfo>();
            }

            if (_onDeserializedMethods.ContainsKey(targetType))
            {
                return _onDeserializedMethods[targetType];
            }

            var methodInfo = targetType.GetMethods(_bindingFlags)
                .Select(targetTypeMethodInfo => new
                {
                    targetTypeMethodInfo,
                    attrs = targetTypeMethodInfo.GetCustomAttributes(typeof(OnDeserializedAttribute), false)
                })
                .Where(t => t.attrs.Length > 0)
                .Select(t => t.targetTypeMethodInfo)
                .ToList();

            _onDeserializedMethods.Add(targetType, methodInfo);
            return methodInfo;
        }
    }
}