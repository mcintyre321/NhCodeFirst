using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Iesi.Collections.Generic;
using NHibernate.Dialect;
using NHibernate.Mapping;
using urn.nhibernate.mapping.Item2.Item2;
using Xml.Schema.Linq;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateNonCompositeIdentity : IClassConvention
    {
        public void Apply(Type entityType, @class classElement, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            //use reflection to get the Id property from the current class
            var idMember = entityType.GetFieldsAndProperties().SingleOrDefault(e => e.Name.ToLower() == "id" || e.GetCustomAttributes(typeof(KeyAttribute), false).Any());
            if (idMember != null)
            {

                var idType = idMember.ReturnType();
                //if the id property exists, add a new id element to the @class element
                classElement.id = new id()
                {
                    name = idMember.Name.Sanitise(),
                    column = { new column()
                        .Setup(idMember)
                        .Apply(c => c.notnull = true)
                        .Apply(c => c.index = "PK_" + classElement.table + "_" + idMember.Name.Sanitise()) }
                };

                if (CanUseHiloGenerator(idType)) //if is integer of some kind
                {
                    classElement.id.generator = new generator()
                    {
                        @class = "hilo",
                        param =
                            {
                                param.Parse(@"<param name=""max_lo"" xmlns=""urn:nhibernate-mapping-2.2"" >10</param>"),
                                param.Parse(@"<param name=""where"" xmlns=""urn:nhibernate-mapping-2.2"" >entity = '" + classElement.table + "'</param>"),
                                param.Parse(@"<param name=""table"" xmlns=""urn:nhibernate-mapping-2.2"" >HiloValues</param>"),
                                

                            }
                    };
                    this.entities.Add(classElement.table);
                }
                else if (idType == typeof(Guid))
                {
                    classElement.id.generator = new generator() { @class = "guid.comb"} ;
                }

            }

        }

        private bool CanUseHiloGenerator(Type idType)
        {
            return new[] { typeof(int), typeof(long) }.Contains(idType);
        }

        List<string> entities = new List<string>();


        public IEnumerable<IAuxiliaryDatabaseObject> AuxDbObjects()
        {
            var script = new StringBuilder(1024);
            script.AppendLine("IF NOT EXISTS (select * from sys.columns where Name = N'Entity'  and Object_ID = Object_ID(N'HiloValues')) BEGIN");
            script.AppendLine("    DELETE FROM HiloValues;");

            script.AppendLine("    ALTER TABLE HiloValues ADD Entity VARCHAR(128) NOT NULL;");
            script.AppendLine("    CREATE NONCLUSTERED INDEX IdxHiloValuesEntity ON HiloValues (Entity ASC);");
            script.AppendLine("END");
            yield return new SimpleAuxiliaryDatabaseObject(script.ToString(), null, new HashedSet<string> { typeof(MsSql2000Dialect).FullName, typeof(MsSql2005Dialect).FullName, typeof(MsSql2008Dialect).FullName });
            script.Clear();
            foreach (var entity in entities) //.Where(x => orm.IsRootEntity(x)))
            {
                script.AppendLine(string.Format("IF NOT EXISTS(SELECT * FROM HiloValues WHERE Entity = '{0}') BEGIN",  entity ));
                script.AppendLine(string.Format("    INSERT INTO [HiloValues] (Entity, next_hi) VALUES ('{0}',1);", entity));
                script.AppendLine(string.Format("END", entity));
            }
            yield return new SimpleAuxiliaryDatabaseObject(script.ToString(), null, new HashedSet<string> { typeof (MsSql2000Dialect).FullName, typeof (MsSql2005Dialect).FullName, typeof (MsSql2008Dialect).FullName });
        }
    }
}