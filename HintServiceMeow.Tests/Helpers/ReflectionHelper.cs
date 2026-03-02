using System;
using System.Reflection;

namespace HintServiceMeow.Tests.Helpers
{
    public static class ReflectionHelper
    {
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found on {obj.GetType().Name}");
            return (T)field.GetValue(obj)!;
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found on {obj.GetType().Name}");
            field.SetValue(obj, value);
        }

        public static void SetStaticProperty(Type type, string propertyName, object value)
        {
            var prop = type.GetProperty(propertyName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            prop?.SetValue(null, value);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null)
                throw new ArgumentException($"Property '{propertyName}' not found on {obj.GetType().Name}");
            return (T)prop.GetValue(obj);
        }
    }
}
