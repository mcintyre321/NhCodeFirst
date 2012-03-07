using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NhCodeFirst.Conventions;

namespace NhCodeFirst.NhCodeFirst
{
    public class SerializedUserTypeAttribute : Attribute
    {
        static SerializedUserTypeAttribute()
        {
            CreateBasicProperties.GetTypeForPropertyRules.Insert(mi => mi.TryGetAttribute<SerializedUserTypeAttribute>() != null ? typeof(SerializedUserType<>).MakeGenericType(mi.ReturnType()) : null);
        }
    }
    public class SerializedUserType<T> : BaseImmutableUserType<T>
    {
        public static Func<string, object> Deserialize = s => Convert.ChangeType(s, typeof (T));
        public static Func<object, string> Serialize = o => o.ToString();

        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var amount = Deserialize(NHibernateUtil.String.NullSafeGet(rs, names[0]) as string);
            return amount;
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            object valueToSet = Serialize(value);
            NHibernateUtil.String.NullSafeSet(cmd, valueToSet, index);
        }

        public override SqlType[] SqlTypes
        {
            get { return new SqlType[]{new StringSqlType(), }; }
        }
    }
}