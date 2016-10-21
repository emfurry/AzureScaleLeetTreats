using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AzureScaleLeetTreats.Web.Utilities
{
    public static class ReflectionUtil
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> _propertiesByType = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static PropertyInfo[] GetPropertiesForType(Type type)
        {
            PropertyInfo[] properties;
            if (!_propertiesByType.TryGetValue(type, out properties))
            {
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                _propertiesByType.TryAdd(type, properties);
            }
            return properties;
        }

        public static string[] GetPropertyNames(Type type)
        {
            var properties = GetPropertiesForType(type);
            return properties.Select(p => p.Name).ToArray();
        }

        public static object GetValue(string propertyName, object obj)
        {
            Type type = obj.GetType();
            var properties = GetPropertiesForType(type);
            var property = properties.Where(p => p.Name == propertyName).Single();
            object value = property.GetValue(obj);
            return value;
        }

        public static T LoadObjectFromReader<T>(IDataReader reader) where T : new()
        {
            T obj = new T();
            var properties = GetPropertiesForType(typeof(T));
            for (int x = 0; x < reader.FieldCount; x++)
            {
                object value = reader[x];
                properties[x].SetValue(obj, value);
            }
            return obj;
        }
    }
}