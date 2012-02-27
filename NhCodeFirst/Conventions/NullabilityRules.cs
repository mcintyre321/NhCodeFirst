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
        public static List<Func<MemberInfo, bool?>> NullableRules = new List<Func<MemberInfo, bool?>>()
        {
            mi => !mi.ReturnType().IsNullableType() ? false : null as bool?, //if a type isn't nullable, then the column can't be nullable!
        };

        public static void AddNotNullAttribute<T>() where T : Attribute
        {
            NullableRules.Add(mi => mi.ReturnType().HasAttribute<T>() ? false : null as bool?);
        }
    }
}