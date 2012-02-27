using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using NHibernate.Mapping;
using NHibernate.Type;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.Conventions
{
    public class CreateBasicProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        public static readonly IList<Type> BasicTypes = new[] { typeof(Guid), typeof(int), typeof(string), typeof(bool), typeof(DateTime), typeof(DateTimeOffset), typeof(Byte[]), typeof(Double), }.ToList().AsReadOnly();

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            var memberInfos = type.GetSettableFieldsAndProperties()
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

        internal static property GetProperty(MemberInfo memberInfo, string prefix = "", Func<MemberInfo, bool> notNull = null)
        {
            if (memberInfo.IsReadOnlyProperty()) return null;
            if (memberInfo.DeclaringType.GetSettableFieldsAndProperties().Any(memberInfo.IsBackingFieldFor)) return null;

            notNull = notNull ?? (mi => !mi.IsNullable());
            var returnType = memberInfo.ReturnType();
            var userType = Type.GetType(returnType.FullName + "UserType" + ", " + returnType.Assembly.FullName);

            var typeOrNonNullableType = returnType.GetTypeOrNonNullableType();
            if (!BasicTypes.Any(t => typeOrNonNullableType == t) && !returnType.IsEnum && userType == null)
                return null;

            var property = new property()
                               {
                                   name = memberInfo.Name,
                                   access = memberInfo.Access(),
                                   notnull = notNull(memberInfo),
                               };
            property.column.Add(new column().Setup(memberInfo, columnPrefix: prefix, notnull: property.notnull));
                                   
            if (userType != null)
            {
                property.type1 = userType.AssemblyQualifiedName;
            }

            UniqueAttribute.SetUniqueProperties(memberInfo, property);

            //this if statement could be happily replaced by some nifty lookup table or something
            if (returnType == typeof(DateTimeOffset) || returnType == typeof(DateTimeOffset?))
            {
                property.type1 = "datetimeoffset";
            }
            if (returnType == typeof(DateTime) || returnType == typeof(DateTime?))
            {
                property.type1 = "UtcDateTime";
            }
            if (returnType.IsEnum)
            {
                property.type1 =
                    typeof (EnumStringType<>).MakeGenericType(returnType).AssemblyQualifiedName;
            }
            return property;
        }

        public IEnumerable<IAuxiliaryDatabaseObject> AuxDbObjects()
        {
            yield break;
        }

        
    }
}