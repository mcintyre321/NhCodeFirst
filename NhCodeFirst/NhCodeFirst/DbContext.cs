using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Linq;
using Configuration = NHibernate.Cfg.Configuration;

namespace NhCodeFirst.NhCodeFirst
{
    //This class is similar to the EF DbContext - you don't have to use it to
    //use the ConfigurationBuilder and conventions but it should help some
    //EF users get started
    abstract class DbContext : IDisposable
    {
        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;

        public ISession Session { get; private set; }
        public DbContext():this(null)
        {
            
        }
        public DbContext(string connectionString)
        {
            if (_sessionFactory == null)
            {
                var cb = new ConfigurationBuilder();

                var entityTypes =
                    from member in this.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                    where typeof (DbSetBase).IsAssignableFrom(member.ReturnType())
                    let genericParameterType = member.ReturnType().GetGenericArguments().Single()
                    select genericParameterType;
                connectionString = connectionString ?? ConfigurationManager.ConnectionStrings[GetType().Name].ConnectionString;
                _configuration = cb.Build(connectionString, entityTypes);
                _sessionFactory = _configuration.BuildSessionFactory();
            }
            Session = _sessionFactory.OpenSession();
        }

        public void Dispose()
        {
            Session.Dispose();
        }
    }

    internal class DbSetBase
    {
    }

    class DbSet<TEntity> : DbSetBase where TEntity : class
    {
        private readonly ISession _session;

        public DbSet(ISession session)
        {
            _session = session;
        }
         
        public void Add(TEntity item)
        {
            _session.SaveOrUpdate(item);
        } 

        public IQueryable<TEntity> Count
        {
            get { throw new NotImplementedException(); }
        }
    }
}