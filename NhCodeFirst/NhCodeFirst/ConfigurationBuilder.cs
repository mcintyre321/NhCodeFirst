using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using DependencySort;
using NhCodeFirst.NhCodeFirst.Conventions;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using urn.nhibernate.mapping.Item2.Item2;
using Environment = NHibernate.Cfg.Environment;

namespace NhCodeFirst.NhCodeFirst
{
    public interface IConfigurationNeedingDialect
    {
        IConfigurationNeedingEntities ForSql2008(string connectionString);
        IConfigurationNeedingEntities ForInMemorySqlite();
    }

    public interface IConfigurationNeedingEntities
    {
        Configuration MapEntities(IEnumerable<Type> rootEntityTypes, MatchEntities matchEntities = null);
        IConfigurationNeedingEntities With(Action<Configuration> transform);
    }

    public class MatchEntities
    {
        public Func<Type, bool> IsEntityMatch
        {
            get { return t => filters.All(f => f(t)); }
        }

        protected MatchEntities(Func<Type, bool> isEntityMatch)
        {
            filters.Add(isEntityMatch);
        }

        public static MatchEntities All { get { return new MatchEntities((t) => true); } }
        public static MatchEntities WithIdProperty { get { return new MatchEntities((t) => t.GetProperty("Id") != null); } }

        public MatchEntities Where(Func<Type, bool> func)
        {
            this.filters.Add(func);
            return this;
        }
        List<Func<Type, bool>> filters = new List<Func<Type, bool>>();
    }

    public class ConfigurationBuilder : IConfigurationNeedingEntities, IConfigurationNeedingDialect
    {

        private readonly Configuration _cfg;
        private ConfigurationBuilder()
        {
            _cfg = new Configuration();
        }
        public static IConfigurationNeedingDialect New()
        {
            return new ConfigurationBuilder();
        }



        #region Dialect methods
        public IConfigurationNeedingEntities With(Action<Configuration> transform)
        {
            transform(_cfg);
            return this;
        }

        #endregion

        #region Dialect methods
        public IConfigurationNeedingEntities ForSql2008(string connectionString)
        {
            SqlDialect.Current = SqlDialect.Default;

            _cfg
                .SetProperty(Environment.Dialect, typeof(NHibernate.Dialect.MsSql2008Dialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver")
                .SetProperty(Environment.ConnectionString, connectionString)
                .SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
            return this;
        }
        public IConfigurationNeedingEntities ForInMemorySqlite()
        {
            SqlDialect.Current = SqlDialect.SQLite;

            _cfg
                .SetProperty(Environment.ReleaseConnections, "on_close")
                .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, "data source=:memory:");
            return this;
        }
        #endregion


        IEnumerable<Type> GetEntityTypes(IEnumerable<Type> rootEntityTypes, MatchEntities matchEntities)
        {
            var typesToBeChecked = new Queue<Type>(rootEntityTypes);

            var checkedTypes = new HashSet<Type>();
            var entityTypes = new HashSet<Type>();

            do
            {
                var typeToBeChecked = typesToBeChecked.Dequeue();

                if ((matchEntities ?? MatchEntities.All).IsEntityMatch(typeToBeChecked))
                    entityTypes.Add(typeToBeChecked);

                if (matchEntities != null)
                {
                    var relatedEntities = typeToBeChecked.GetAllMembers()
                        .Where(m => m.IsReadOnlyField() == false)
                        .Select(m => m.ReturnType())
                        .Where(m => m != null)
                        .Select(t => t.GetTypeOrGenericArgumentTypeForICollection())
                        .Select(t => t.GetTypeOrGenericArgumentTypeForIQueryable())
                        .Where(m => m != null);

                    foreach (var e in relatedEntities)
                    {
                        if (matchEntities.IsEntityMatch(e))
                        {
                            entityTypes.Add(e);
                        }

                        if (checkedTypes.Add(e))
                        {
                            typesToBeChecked.Enqueue(e);
                        }
                    }
                }
            } while (typesToBeChecked.Any());
            return entityTypes;
        }

        public Configuration MapEntities(IEnumerable<Type> rootEntityTypes, MatchEntities matchEntities = null)
        {
            var entityTypes = GetEntityTypes(rootEntityTypes, matchEntities);

            var mappingXDoc = new hibernatemapping(); //this creates the mapping xml document

            //create class xml elements for each entity type
            foreach (var type in entityTypes)
            {
                var @class = new @class()
                {
                    name = type.AssemblyQualifiedName,
                    table = type.Name.Pluralize(), //pluralized table names - could easily have checked for a [TableName("SomeTable")] attribute for custom overrides
                };
                mappingXDoc.@class.Add(@class);
            }

            var conventions =
                GetAll<IClassConvention>() //get all the conventions from the current project
                    .TopologicalSort() //sort them into a dependency tree
                    .ToList();

            //run througn all the conventions, updating the document as we go
            foreach (var convention in conventions)
            {
                foreach (var type in entityTypes)
                {
                    var @class = mappingXDoc.@class.Single(c => c.name == type.AssemblyQualifiedName);
                    convention.Apply(type, @class, entityTypes, mappingXDoc);
                }
            }

            var xml = mappingXDoc.ToString();
#if DEBUG
            var path = HostingEnvironment.ApplicationPhysicalPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").Replace("/", "\\"));
            File.WriteAllText(Path.Combine(path, "config.hbm.xml"), xml);
#endif
            _cfg.AddXml(xml);
            return _cfg;
        }

        static IEnumerable<T> GetAll<T>()
        {
            var unsortedConventionTypes = typeof(T).Assembly.GetTypesSafe()
                 .Where(t => typeof(T).IsAssignableFrom(t))
                 .Where(t => t.CanBeInstantiated());


            var conventionTypes = unsortedConventionTypes.TopologicalSort();
            return conventionTypes.Select(t => (T)Activator.CreateInstance(t)).ToList();
        }

        public Configuration Build(string connectionString, IEnumerable<Type> entityTypes)
        {
            var cfg = new ConfigurationBuilder()
                .ForSql2008(connectionString)
                .MapEntities(entityTypes);
            return cfg;
        }
    }
}