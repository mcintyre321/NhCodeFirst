using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NHibernate.Dialect;
using NHibernate.Type;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    static class ColumnExtensions
    {
        public static column Setup(this column column, MemberInfo memberInfo, string columnName = null, string columnPrefix = "", bool? notnull = true)
        {
            column.SetName(columnName ?? column.GetName() ?? memberInfo.Name.Sanitise(), columnPrefix);

            if (memberInfo.ReturnType() == typeof(string))
            {
                var stringLengthAttribute = memberInfo.GetCustomAttributes(true).OfType<StringLengthAttribute>().SingleOrDefault();
                string maxLength = stringLengthAttribute == null ? SqlDialect.Current.VarcharMax : stringLengthAttribute.MaximumLength.ToString();
                column.sqltype = "NVARCHAR(" + maxLength + ")";
            }
            if (memberInfo.ReturnType() == typeof(byte[]))
            {
                column.sqltype = "VARBINARY(MAX)";
            }

            column.notnull = notnull;

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

    public class SqlDialect
    {
        public static SqlDialect Default
        {
            get { return SqlDialect.MsSql; }
        }

        protected static SqlDialect MsSql = new SqlDialect()
        {
            VarcharMax = "MAX"
        };

        public static SqlDialect SQLite = new SqlDialect()
        {
            VarcharMax = int.MaxValue.ToString()
        };


        public string VarcharMax { get; private set; }

        public static SqlDialect Current { get; set; }
    }

    static class Extensions
    {
        public static @class ClassElement(this Type type, hibernatemapping hbm)
        {
            return hbm.@class.SingleOrDefault(c => c.name == type.AssemblyQualifiedName);
        }

        public static string Access(this MemberInfo memberInfo)
        {
            var backingField = memberInfo.DeclaringType.GetSettableFieldsAndProperties()
                .SingleOrDefault(f => f.IsBackingFieldFor(memberInfo));

            if (memberInfo.MemberType == MemberTypes.Field)
            {
                Debug.Assert(backingField == null, "If this is a field, it shouldn't have a backing field (only properties do)");
                return "field";
            }
            //so we must be a property...
            if (backingField == null)
            {
                return null; //vanilla property
            }
            var access = "field.camelcase";
            if (backingField.Name.StartsWith("_")) access += "-underscore";
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

        public static bool IsNullableType(this Type theType)
        {
            return theType.IsClass || theType.IsInterface || theType.IsNullableValueType();
        }

        public static Type GetTypeOrNonNullableType(this Type type)
        {
            return IsNullableValueType(type) ? type.GetGenericArguments()[0] : type;
        }

        private static bool IsNullableValueType(this Type type)
        {
             return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
