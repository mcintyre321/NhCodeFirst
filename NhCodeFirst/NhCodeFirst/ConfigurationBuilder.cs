using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using DependencySort;
using NhCodeFirst.NhCodeFirst.Conventions;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;
using urn.nhibernate.mapping.Item2.Item2;
using Environment = NHibernate.Cfg.Environment;

namespace NhCodeFirst.NhCodeFirst
{
    public class ConfigurationBuilder
    {
        public Configuration Build(string connectionString, IEnumerable<Type> entityTypes)
        {
            var cfg = new Configuration(); //create the configuration object 

            cfg.SetProperty(Environment.Dialect, typeof(NHibernate.Dialect.MsSql2008Dialect).AssemblyQualifiedName);
            cfg.SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver");
            cfg.SetProperty(Environment.ConnectionString, connectionString);
            cfg.SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
            cfg.SetProperty(Environment.ProxyFactoryFactoryClass, typeof(ProxyFactoryFactory).AssemblyQualifiedName);


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
            File.WriteAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "config.hbm.xml"), xml);
            cfg.AddXml(xml);
            return cfg;
        }

        //This method returns all instances of T that are defined in the assembly
        //You might want to switch this out for a call to your IOC container.
        static IEnumerable<T> GetAll<T>()
        {
            var unsortedConventionTypes = typeof(T).Assembly.GetTypesSafe()
                 .Where(t => typeof(T).IsAssignableFrom(t))
                 .Where(t => t.CanBeInstantiated());


            var conventionTypes = unsortedConventionTypes.TopologicalSort();
            return conventionTypes.Select(t => (T)Activator.CreateInstance(t)).ToList();
        }
    }
}