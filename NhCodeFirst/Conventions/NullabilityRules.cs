using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NhCodeFirst.Conventions
{
    public static class NullabilityRules
    {
        static NullabilityRules()
        {
            AddNotNullAttribute<RequiredAttribute>();
            AddNotNullAttribute<KeyAttribute>();
            AddNotNullAttribute<UniqueAttribute>();
        }
        public static List<Func<MemberInfo, bool?>> IsNullRules = new List<Func<MemberInfo, bool?>>()
        {
            mi => !mi.ReturnType().IsNullableType() ? false : null as bool?, //if a type isn't nullable, then the column can't be nullable!
        };

        public static void AddNotNullAttribute<T>() where T : Attribute
        {
            IsNullRules.Add(mi => mi.ReturnType().HasAttribute<T>() ? false : null as bool?);
        }

        public static bool IsNullable(this MemberInfo memberInfo)
        {
            foreach (var nullableRule in NullabilityRules.IsNullRules)
            {
                var result = nullableRule(memberInfo);
                if (result.HasValue) return result.Value;
            }
            return true;
        }
    }
}