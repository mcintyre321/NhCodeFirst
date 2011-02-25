using System;
using System.Reflection;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    static class Extensions
    {
        public static string Access(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Field ? "field.camelcase" : null;
        }
        public static string Capitalise(this string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }
        public static string StripUnderscores(this string s)
        {
            return s.Trim('_');
        }
        public static string Sanitise(this string s)
        {
            return s.StripUnderscores().Capitalise();
        }
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }
    }
}
