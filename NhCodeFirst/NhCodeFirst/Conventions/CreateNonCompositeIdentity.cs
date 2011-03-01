using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateNonCompositeIdentity : IClassConvention
    {
        public void Apply(Type entityType, @class classElement, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            //use reflection to get the Id property from the current class
            var idMember = entityType.GetFieldsAndProperties()
                .Where(e => e.Name.ToLower() == "id" || e.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .SingleOrDefault();
            if (idMember != null)
            {
                var idType = idMember.ReturnType();
                //if the id property exists, add a new id element to the @class element
                classElement.id = new id() 
                {
                    name = idMember.Name.Sanitise(),
                    column = { new column().Setup(idMember).Apply(c => c.notnull = true) }
                };

                if (CanUseHiloGenerator(idType)) //if is integer of some kind
                {
                    classElement.id.generator = new generator() {@class = "hilo"};
                }
            }
        }

        private bool CanUseHiloGenerator(Type idType)
        {
            return new[]{typeof(int), typeof(long)}.Contains(idType);
        }
    }
}