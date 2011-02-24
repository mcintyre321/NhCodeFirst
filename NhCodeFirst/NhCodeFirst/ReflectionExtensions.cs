using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Linq;
using System.Reflection;

namespace NhCodeFirst.NhCodeFirst
{
    public static class Extensions
    {
        public static string Pluralize(this string text)
        {
            var service = PluralizationService.CreateService(System.Threading.Thread.CurrentThread.CurrentUICulture);
            return service.Pluralize(text);
        }

        public static BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static IEnumerable<MemberInfo> GetAllMembers(this Type type)
        {
            return type.GetMembers(BindingFlags).Where(p => p.IsBackingField() == false);
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