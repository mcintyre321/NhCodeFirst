using System;
using System.Linq;
using System.Reflection;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class UniqueAttribute : Attribute
    {

        public string KeyName { get; private set; }
        public bool NotNull { get; private set; }

        public UniqueAttribute()
            : this(null, true)
        {

        }
        public UniqueAttribute(bool notNull)
            : this(null, notNull)
        {

        }
        public UniqueAttribute(string keyName, bool notNull = true)
        {
            KeyName = keyName;
            NotNull = notNull;
        }

        internal static void SetUniqueProperties(MemberInfo memberInfo, property p)
        {
            SetUniqueProperties(memberInfo, ua =>
            {
                p.uniquekey = ua.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
                var column = p.column.SingleOrDefault();
                if (column != null)
                {
                    column.notnull = ua.NotNull;
                    column.uniquekey = ua.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
                }else
                {
                    p.notnull = ua.NotNull;
                    p.uniquekey = ua.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
                }
                

            });
        }
        internal static void SetUniqueProperties(MemberInfo memberInfo, manytoone manytoone)
        {
            SetUniqueProperties(memberInfo, ua =>
            {
                var column = manytoone.column.SingleOrDefault();
                if (column != null)
                {
                    column.notnull = ua.NotNull;
                    column.uniquekey = ua.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
                }
                else
                {
                    manytoone.notnull = ua.NotNull;
                    manytoone.uniquekey = ua.KeyName ?? memberInfo.DeclaringType.Name + "_UniqueKey";
                }

            });
        }

        internal static void SetUniqueProperties(MemberInfo memberInfo, Action<UniqueAttribute> makeUnique)
        {
            var uniqueAttribute = memberInfo.TryGetAttribute<UniqueAttribute>();
            if (uniqueAttribute == null)
                return;
            makeUnique(uniqueAttribute);
        }
    }
}