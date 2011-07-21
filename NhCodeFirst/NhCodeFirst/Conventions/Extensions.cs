using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NHibernate.Type;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    static class ColumnExtensions
    {
        public static column Setup(this column column, MemberInfo memberInfo, string columnName = null, string columnPrefix = "")
        {
            column.SetName(columnName ?? column.GetName() ?? memberInfo.Name.Sanitise(), columnPrefix);

            if (memberInfo.ReturnType() == typeof(string))
            {
                var stringLengthAttribute = memberInfo.GetCustomAttributes(true).OfType<StringLengthAttribute>().SingleOrDefault();
                string maxLength = stringLengthAttribute == null ? "MAX" : stringLengthAttribute.MaximumLength.ToString();
                column.sqltype = "NVARCHAR(" + maxLength + ")";
            }
            if (memberInfo.ReturnType() == typeof(byte[]))
            {
                column.sqltype = "VARBINARY(MAX)";
            }

            column.notnull = !memberInfo.Nullable();

            return column;
        }
        public static column SetName(this column column, string name, string columnPrefix = "")
        {
            column.name = '[' + columnPrefix + name.Trim('[', ']') + ']';
            return column;
        }
        public static string GetName(this column column)
        {
            return column.name == null ? null : column.name.Trim('[', ']');
        } 

    }
    static class Extensions
    {
        public static @class ClassElement(this Type type, hibernatemapping hbm)
        {
            return hbm.@class.SingleOrDefault(c => c.name == type.AssemblyQualifiedName);
        }

        public static string Access(this MemberInfo memberInfo)
        {

            var access = memberInfo.MemberType == MemberTypes.Field ? "field.camelcase" : null;
            //if (memberInfo.Name.StartsWith("_") && access != null) access += "-underscore";
            return access;
        }

        public static bool Nullable(this MemberInfo memberInfo)
        {
            var type = memberInfo.ReturnType();
            return (type.IsNullableType())
                   && !type.HasAttribute<RequiredAttribute>()
                   && !type.HasAttribute<KeyAttribute>()
                   && !type.HasAttribute<UniqueAttribute>();

        }
        public static bool HasAttribute<T>(this MemberInfo mi)
        {
            return mi.GetCustomAttributes(typeof (T), false).Any();
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

        private static bool IsNullableType(this Type theType)
        {
            return theType.IsClass || theType.IsInterface || (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }
    }
}
