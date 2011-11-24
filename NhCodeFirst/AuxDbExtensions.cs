using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate;
using NHibernate.Cfg;

namespace NhCodeFirst
{
    public static class AuxDbExtensions
    {
        public static void ExecuteAuxilliaryDatabaseScripts(this Configuration configuration, ISessionFactory sf)
        {
            dynamic exposed = new ExposedObjectSimple(configuration);
            foreach (var aux in exposed.auxiliaryDatabaseObjects)
            {
                var script = ((dynamic)new ExposedObjectSimple(aux)).sqlCreateString;
                using (var s = sf.OpenStatelessSession())
                {
                    s.CreateSQLQuery((string)script).ExecuteUpdate();
                }
            }
        }
    }
    class ExposedObjectSimple : DynamicObject
    {
        private object m_object;

        public ExposedObjectSimple(object obj)
        {
            m_object = obj;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var fieldInfo = m_object.GetType().GetField(binder.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            result = fieldInfo.GetValue(m_object);
            return true;
        }
        public override bool TryInvokeMember(
                InvokeMemberBinder binder, object[] args, out object result)
        {
            // Find the called method using reflection
            var methodInfo = m_object.GetType().GetMethod(binder.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            // Call the method
            result = methodInfo.Invoke(m_object, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
            return true;
        }
    }
}
