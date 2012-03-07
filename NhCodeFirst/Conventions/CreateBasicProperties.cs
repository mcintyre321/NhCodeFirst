using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using NHibernate.Mapping;
using NHibernate.Type;
using NhCodeFirst.UserTypes;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.Conventions
{
    public class DontMapAttribute
    {
    }
    public class CreateBasicProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        public static Registry<GetTypeForProperty> GetTypeForPropertyRules = new Registry<GetTypeForProperty>();
        static CreateBasicProperties()
        {
            GetTypeForPropertyRules.Insert(mi => BasicTypes.Any(t => t == mi.ReturnType().GetTypeOrUnderlyingType()) ? mi.ReturnType() : null);
            GetTypeForPropertyRules.Insert(mi => mi.TryGetAttribute<DontMapAttribute>() != null ? typeof(void) : null);
            GetTypeForPropertyRules.Insert(mi => mi.TryGetAttribute<SerializedUserTypeAttribute>() != null ? typeof(SerializedUserType<>).MakeGenericType(mi.ReturnType()) : null);
        }
        public static readonly IList<Type> BasicTypes = new[] { typeof(Guid), typeof(int), typeof(long), typeof(string), typeof(bool), typeof(DateTime), typeof(DateTimeOffset), typeof(Byte[]), typeof(Double), }.ToList().AsReadOnly();

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

        //decide which type to use for the property mapping. Return typeof(void) if the rule should prevent the property from mapping at all
        public delegate Type GetTypeForProperty(MemberInfo mi);

    
        internal static property GetProperty(MemberInfo memberInfo, string prefix = "", Func<MemberInfo, bool> notNull = null)
        {
            if (memberInfo.IsReadOnlyProperty()) return null;
            if (memberInfo.DeclaringType.GetSettableFieldsAndProperties().Any(memberInfo.IsBackingFieldFor)) return null;

            notNull = notNull ?? (mi => !mi.IsNullable());
            var returnType = memberInfo.ReturnType();
            var type = GetTypeForPropertyRules.Rules.Select(r => r(memberInfo)).FirstOrDefault(t => t != null);
            if (type == null || type == typeof(void)) return null; //not for mapping, this one

            var property = new property()
                               {
                                   name = memberInfo.Name,
                                   access = memberInfo.Access(),
                                   notnull = notNull(memberInfo),
                               };
            property.column.Add(new column().Setup(memberInfo, columnPrefix: prefix, notnull: property.notnull));

            property.type1 = type.AssemblyQualifiedName;
            
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