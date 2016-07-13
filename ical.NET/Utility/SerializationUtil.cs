using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ical.Net.Utility
{
    public class SerializationUtil
    {
        public static void OnDeserializing(object obj)
        {
            foreach (var mi in GetDeserializingMethods(obj.GetType()))
            {
                mi.Invoke(obj, new object[] {new StreamingContext() });
            }
        }

        public static void OnDeserialized(object obj)
        {
            foreach (var mi in GetDeserializedMethods(obj.GetType()))
            {
                mi.Invoke(obj, new object[] { new StreamingContext() });
            }
        }

        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly ConcurrentDictionary<Type, List<MethodInfo>> _onDeserializingMethods = new ConcurrentDictionary<Type, List<MethodInfo>>();
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
                    attrs = targetTypeMethodInfo.GetCustomAttributes(typeof(OnDeserializingAttribute), false).ToList()
                })
                .Where(t => t.attrs.Count > 0)
                .Select(t => t.targetTypeMethodInfo)
                .ToList();

            _onDeserializingMethods.AddOrUpdate(targetType, methodInfo, (type, list) => methodInfo);
            return methodInfo;
        }

        private static ConcurrentDictionary<Type, List<MethodInfo>> _onDeserializedMethods = new ConcurrentDictionary<Type, List<MethodInfo>>();
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
                    attrs = targetTypeMethodInfo.GetCustomAttributes(typeof(OnDeserializedAttribute), false).ToList()
                })
                .Where(t => t.attrs.Count > 0)
                .Select(t => t.targetTypeMethodInfo)
                .ToList();

            _onDeserializedMethods.AddOrUpdate(targetType, methodInfo, (type, list) => methodInfo);
            return methodInfo;
        }
    }
}