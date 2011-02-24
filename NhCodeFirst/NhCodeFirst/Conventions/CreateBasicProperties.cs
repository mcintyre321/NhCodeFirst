using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DependencySort;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateBasicProperties : IClassConvention, IRunAfter<CreateHiloIdIfTypeHasIntId>, IRunAfter<AddVersion>
    {
        public static readonly IList<Type> BasicTypes = new[] { typeof(Guid), typeof(Guid?), typeof(int), typeof(int?), typeof(string), typeof(bool), typeof(bool?), typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(Byte[]) }.ToList().AsReadOnly();

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping mapping)
        {
            var memberInfos = type.GetAllMembers()
                .Where(p => p.Name != @class.id.name)
                .Where(p => @class.version == null || p.Name != @class.version.name)
                .Where(p => BasicTypes.Contains(p.ReturnType()))
                .Where(p => !p.Name.Contains("BackingField"));
            foreach (var memberInfo in memberInfos)
            {
                var property = new property()
                {
                    name = memberInfo.Name.Capitalise(),
                    column = { new column() { name = "[" + memberInfo.Name.Capitalise() + "]" } },
                    access = memberInfo.Access(),
                    notnull = !memberInfo.ReturnType().IsNullableType()
                };
                if (memberInfo.ReturnType() == typeof(byte[]))
                {
                    property.column[0].sqltype = "VARBINARY(MAX)";
                }
                if (memberInfo.ReturnType() == typeof(DateTimeOffset) || memberInfo.ReturnType() == typeof(DateTimeOffset?))
                {
                    property.type1 = "datetimeoffset";
                }
                if (memberInfo.ReturnType() == typeof(DateTime) || memberInfo.ReturnType() == typeof(DateTime?))
                {
                    property.type1 = "UtcDateTime";
                }
                if (memberInfo.ReturnType() == typeof(string))
                {
                    var stringLengthAttribute = memberInfo.GetCustomAttributes(true).OfType<StringLengthAttribute>().SingleOrDefault();
                    string maxLength = stringLengthAttribute == null ? "MAX" : stringLengthAttribute.MaximumLength.ToString();
                    property.column.Single().sqltype = "NVARCHAR(" + maxLength + ")";
                }


                @class.property.Add(property);

            }
        }
        
    }
}