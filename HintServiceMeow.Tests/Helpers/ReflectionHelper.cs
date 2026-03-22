using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HintServiceMeow.Tests.Helpers
{
    public static class ReflectionHelper
    {
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found on {obj.GetType().Name}");
            return (T)field.GetValue(obj);
        }

        public static T GetFieldValueFromParent<T>(object obj, string fieldName)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                    return (T)field.GetValue(obj);
                type = type.BaseType;
            }
            throw new ArgumentException($"Field '{fieldName}' not found on {obj.GetType().Name} or its parents");
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found on {obj.GetType().Name}");
            field.SetValue(obj, value);
        }

        public static T GetStaticFieldValue<T>(Type type, string fieldName)
        {
            var field = type.GetField(
                fieldName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Static field '{fieldName}' not found on {type.Name}");
            return (T)field.GetValue(null);
        }

        public static void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            var field = type.GetField(
                fieldName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Static field '{fieldName}' not found on {type.Name}");
            field.SetValue(null, value);
        }

        public static void SetStaticProperty(Type type, string propertyName, object value)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
                throw new ArgumentException($"Static property '{propertyName}' not found on {type.Name}");
            prop.SetValue(null, value);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null)
                throw new ArgumentException($"Property '{propertyName}' not found on {obj.GetType().Name}");
            return (T)prop.GetValue(obj)!;
        }
    }

    /// <summary>
    /// Reference equality comparer for use in HashSet to detect duplicate object references.
    /// Needed because ReferenceEqualityComparer is not available in net48.
    /// </summary>
    public sealed class ReferenceComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static ReferenceComparer<T> Instance { get; } = new ReferenceComparer<T>();

        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
