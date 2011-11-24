using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using NHibernate.Mapping;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class ComponentAttribute:Attribute
    {
    }

    public class CreateComponentMappedProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        private static IList<Func<MemberInfo, bool>> _identificationRules = new List<Func<MemberInfo, bool>>()
        {
            mi => mi.HasAttribute<ComponentAttribute>(),
            mi => mi.ReturnType().HasAttribute<ComponentAttribute>(),
        };

        public static void AddRuleForIdentifyingComponents(Func<MemberInfo, bool> predicate)
        {
            _identificationRules.Add(predicate);
        }
        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            foreach (var memberInfo in type.GetSettableFieldsAndProperties())
            {
                var component = GetComponent(memberInfo);
                if (component != null) @class.component.Add(component);
            }
        }

         
        component GetComponent(MemberInfo mi, string columnPrefix = "")
        {
            if (!_identificationRules.Any(rule => rule(mi)))
                return null;

            var fields = mi.DeclaringType.GetSettableFieldsAndProperties();
            if (fields.Any(mi.IsBackingFieldFor)) return null; //we don't want to map the wrapping properties


            var component = new component() { name = mi.Name, access= mi.Access()};
            var prefix = columnPrefix + component.name.Sanitise() + "_";
            var fieldsAndProperties = mi.ReturnType().GetSettableFieldsAndProperties().ToList();
            var parent = fieldsAndProperties.SingleOrDefault(p => p.Name.TrimStart('_').ToLower() == "parent");
            if (parent != null)
            {
                fieldsAndProperties.Remove(parent);
                component.parent = new parent() {name = parent.Name, };
            }

            foreach (var memberInfo in fieldsAndProperties)
            {
                var subcomponent = GetComponent(memberInfo, prefix);
                if (subcomponent != null)
                {
                    component.component1.Add(component);
                    continue;
                }
                var property = CreateBasicProperties.GetProperty(memberInfo, prefix, propMi => false);
                if (property != null)
                {
                    component.property.Add(property);
                    continue;
                }
                
            }
            return component;
        }

        public IEnumerable<IAuxiliaryDatabaseObject> AuxDbObjects()
        {
            yield break;
        }

        private static string GetColumnNamePrefix(MemberInfo memberInfo)
        {
            var propName = memberInfo.ReturnType().Name; //suppose we have a property called MembershipInfo...
            return propName + "_";  //...we write it to a column MembershipInfo_...
        }
    }
}