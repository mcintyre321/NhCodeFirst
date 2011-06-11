using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using Configuration = NHibernate.Cfg.Configuration;

namespace NhCodeFirst.NhCodeFirst
{
    //This class is similar to the EF DbContext - you don't have to use it to
    //use the ConfigurationBuilder and conventions but it should help some
    //EF users get started
    public abstract class DbContext : IDisposable
    {
        public enum DbOption
        {
            DropAndRecreate,
            UpdateSchema
        }
        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;

        public ISession Session { get; private set; }
        
        public DbContext(string connectionString, DbOption dbOption)
        {
            if (_sessionFactory == null)
            {
                _configuration = ConfigurationBuilder.New()
                    .ForSql2008(connectionString)
                    .MapEntities(GetEntityTypes());

                CreateOrUpdateDatabaseAndSchema(connectionString, dbOption);

                _sessionFactory = _configuration.BuildSessionFactory();
            }
            Session = _sessionFactory.OpenSession();
        }

        private void CreateOrUpdateDatabaseAndSchema(string connectionString, DbOption dbOption)
        {
            var dbInit = new DatabaseInitializer(connectionString);
            if (dbOption == DbOption.DropAndRecreate || !dbInit.Exists())
            {
                dbInit.Drop();
                dbInit.Create();
                new SchemaExport(_configuration).Execute(true, true, false);
            }
            else
            {
                //attempt to update the schema
                new SchemaUpdate(_configuration).Execute(true, true);
            }
        }

        public abstract IEnumerable<Type> GetEntityTypes();

        public void Dispose()
        {
            Session.Dispose();
        }
    }
}