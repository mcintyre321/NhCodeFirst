using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class ComponentAttribute:Attribute
    {
    }

    public class CreateComponentMappedProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            foreach (var memberInfo in type.GetFieldsAndProperties())
            {
                var component = GetComponent(memberInfo);
                if (component != null) @class.component.Add(component);
            }
        }

         
        component GetComponent(MemberInfo mi, string columnPrefix = "")
        {
            if (!mi.HasAttribute<ComponentAttribute>())
                return null;
            var component = new component() {name = mi.Name.Sanitise()};
            var prefix = columnPrefix + component.name + "_";

            foreach (var memberInfo in mi.ReturnType().GetFieldsAndProperties())
            {
                var subcomponent = GetComponent(memberInfo, prefix);
                if (subcomponent != null)
                {
                    component.component1.Add(component);
                    continue;
                }
                var property = CreateBasicProperties.GetProperty(memberInfo, prefix);
                if (property != null)
                {
                    component.property.Add(property);
                    continue;
                }
                
            }
            return component;
        }

        private static string GetColumnNamePrefix(MemberInfo memberInfo)
        {
            var propName = memberInfo.ReturnType().Name; //suppose we have a property called MembershipInfo...
            return propName + "_";  //...we write it to a column MembershipInfo_...
        }
    }
}