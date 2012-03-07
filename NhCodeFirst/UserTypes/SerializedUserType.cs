using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NhCodeFirst.Conventions;

namespace NhCodeFirst.UserTypes
{
    public class SerializedUserTypeAttribute : Attribute
    {
       
    }
    public class SerializedUserType
    {
        public static Func<string, Type, object> Deserialize = (s, t) => Convert.ChangeType(s, t);
        public static Func<object, string> Serialize = o => o.ToString();
    }


    public class SerializedUserType<T> : BaseImmutableUserType<T>
    {

        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var amount = SerializedUserType.Deserialize(NHibernateUtil.String.NullSafeGet(rs, names[0]) as string, typeof(T));
            return amount;
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            object valueToSet = SerializedUserType.Serialize(value);
            NHibernateUtil.String.NullSafeSet(cmd, valueToSet, index);
        }

        public override SqlType[] SqlTypes
        {
            get { return new SqlType[]{new StringSqlType(), }; }
        }
    }
}