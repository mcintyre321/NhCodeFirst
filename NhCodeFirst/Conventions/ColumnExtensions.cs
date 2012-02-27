using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.Conventions
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
}