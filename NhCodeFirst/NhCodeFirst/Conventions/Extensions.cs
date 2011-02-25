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
        public static column Setup(this column column, MemberInfo memberInfo, string columnName = null)
        {
            column.name = columnName ?? column.name ?? memberInfo.Name.Sanitise();
            column.name = '[' + column.name.Trim('[', ']') + ']';

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
            
            return column;
        } 
    }
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
