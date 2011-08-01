using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using DependencySort;
using NHibernate.Type;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateBasicProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        public static readonly IList<Type> BasicTypes = new[] { typeof(Guid), typeof(Guid?), typeof(int), typeof(int?), typeof(string), typeof(bool), typeof(bool?), typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(Byte[]) }.ToList().AsReadOnly();

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            var memberInfos = type.GetFieldsAndProperties()
                .Where(p => p.Name != @class.id.name)
                .Where(p => @class.version == null || p.Name != @class.version.name);
            foreach (var memberInfo in memberInfos)
            {
                var property = GetProperty(memberInfo, @class);
                if (property == null) continue;
                @class.property.Add(property);
            }
        }

        internal static property GetProperty(MemberInfo memberInfo, @class @class)
        {
            return GetProperty(memberInfo);
        }

        internal static property GetProperty(MemberInfo memberInfo, string prefix = "")
        {
            if (memberInfo.IsReadOnlyProperty()) return null;
            if (!(BasicTypes.Contains(memberInfo.ReturnType()) || memberInfo.ReturnType().IsEnum))
                return null;
            var property = new property()
                               {
                                   name = memberInfo.Name,
                                   column = { new column().Setup(memberInfo, columnPrefix: prefix)},
                                   access = memberInfo.Access(),
                                   notnull = !memberInfo.Nullable()
                               };
            SetUniqueProperties(memberInfo, property);

            //this if statement could be happily replaced by some nifty lookup table or something
            if (memberInfo.ReturnType() == typeof(DateTimeOffset) || memberInfo.ReturnType() == typeof(DateTimeOffset?))
            {
                property.type1 = "datetimeoffset";
            }
            if (memberInfo.ReturnType() == typeof(DateTime) || memberInfo.ReturnType() == typeof(DateTime?))
            {
                property.type1 = "UtcDateTime";
            }
            if (memberInfo.ReturnType().IsEnum)
            {
                property.type1 =
                    typeof (EnumStringType<>).MakeGenericType(memberInfo.ReturnType()).AssemblyQualifiedName;
            }
            return property;
        }

        private static void SetUniqueProperties(MemberInfo memberInfo, property property)
        {
            var uniqueAttribute = memberInfo.ReturnType().GetCustomAttributes(typeof (UniqueAttribute), false).SingleOrDefault() as UniqueAttribute;
            if (uniqueAttribute == null)
                return;

            property.uniquekey = uniqueAttribute.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
            property.unique = true;
            property.notnull = true;
        }
    }

    public class UniqueAttribute : Attribute
    {
        public string KeyName { get; private set; }

        public UniqueAttribute()
        {
        }
        public UniqueAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }
}