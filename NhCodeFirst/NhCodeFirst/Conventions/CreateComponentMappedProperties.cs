using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateComponentMappedProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            var classMembers = from mi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  let rt = mi.ReturnType()
                                  where rt != null && rt.IsClass
                                  select mi;
            var nonEntityClassMembers = from mi in classMembers
                                        let rt = mi.ReturnType()
                                        where entityTypes.Contains(rt) == false
                                        && CreateBasicProperties.BasicTypes.Contains(rt) == false
                                        select mi;
            foreach (var memberInfo in nonEntityClassMembers.Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property))
            {                
                @class.component.Add(new component()
                {
                    name =  memberInfo.Name.Capitalise(),
                    property = memberInfo.ReturnType()
                        .GetProperties().Select(mi => new property{ name = mi.Name}).ToList()
                });

            }
        }

        private static string GetColumnNamePrefix(MemberInfo memberInfo)
        {
            var propName = memberInfo.ReturnType().Name; //suppose we have a property called MembershipInfo...
            return propName + "_";  //...we write it to a column MembershipInfo_...
        }
    }
}