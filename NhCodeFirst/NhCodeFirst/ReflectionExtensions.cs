using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using urn.nhibernate.mapping.Item2.Item2;
using Xml.Schema.Linq;

namespace NhCodeFirst.NhCodeFirst
{
    public static class Extensions
    {
        public static T Apply<T>(this T t, Action<T> action)
        {
            action(t);
            return t;
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> collection, Action<T> action) 
        {
            foreach (var item in collection)
            {
                action(item);
                yield return item;
            }
        }

        public static T Copy<T>(this T t)
        {
            var xml = t.ToString();
            var obj = typeof (T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] {xml});
            return (T) obj;
        }

        public static List<T> Copy<T>(this IList<T> list)
        {
            var newList = new List<T>();
            foreach (var l in list)
            {
                newList.Add(l.Copy());
            }
            return newList;
        }
         


        public static string Pluralize(this string text)
        {
            var service = PluralizationService.CreateService(System.Threading.Thread.CurrentThread.CurrentUICulture);
            return service.Pluralize(text);
        }

        public static BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type)
        {
            return
                type.GetAllMembers().Where(
                    m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);
        }
        public static IEnumerable<MemberInfo> GetAllMembers(this Type type)
        {
            return type.GetMembers(BindingFlags).Where(p => p.IsBackingField() == false);
        }

        public static void SetValue(this MemberInfo memberInfo, object obj, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                var propertyInfo = memberInfo.DeclaringType.GetProperty(memberInfo.Name,
                                                                        BindingFlags.NonPublic | BindingFlags.Public |
                                                                        BindingFlags.Instance | BindingFlags.Static);
                propertyInfo.GetSetMethod().Invoke(obj, new object[] { value });
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                var fieldInfo = memberInfo.DeclaringType.GetField(memberInfo.Name,
                                                                       BindingFlags.NonPublic | BindingFlags.Public |
                                                                       BindingFlags.Instance | BindingFlags.Static);
                fieldInfo.SetValue(obj, value);
            }
        }

        public static IEnumerable<Type> MakeGenericTypes(this IEnumerable<Type> openGenerics, Type type)
        {
            return openGenerics.Select(og => og.MakeGenericType(type));
        }

        public static Type ReturnType(this MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                return memberInfo.DeclaringType.GetField(memberInfo.Name, BindingFlags).FieldType;
            }
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                return memberInfo.DeclaringType.GetProperty(memberInfo.Name, BindingFlags).PropertyType;
            }
            return null;
        }
        public static bool Inherits(this Type toCheck, Type generic)
        {
            while (toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
        public static bool Implements(this Type toCheck, Type generic)
        {
            return toCheck.GetInterfaces()
                .Select(@interface => @interface.IsGenericType ? @interface.GetGenericTypeDefinition() : @interface)
                .Any(cur => generic == cur);
        }

        public static bool CanBeInstantiated(this Type type)
        {
            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }
        public static IEnumerable<Type> GetTypesSafe(this Assembly assembly)
        {
            return assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace));
        }
        public static bool ImplementsGeneric<T>(this Type type, Type genericType)
        {
            return typeof (T).MakeGenericType(genericType).IsAssignableFrom(type);
        }

        public static bool IsBackingField(this MemberInfo info)
        {
            return info.Name.ToLower().Contains("backing");
        }
    }
}