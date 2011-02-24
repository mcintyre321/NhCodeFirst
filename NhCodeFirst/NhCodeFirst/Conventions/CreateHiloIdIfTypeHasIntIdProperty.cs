using System;
using System.Collections.Generic;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateHiloIdIfTypeHasIntId : IClassConvention
    {
        public void Apply(Type entityType, @class classElement, IEnumerable<Type> entityTypes, hibernatemapping mapping)
        {
            //use reflection to get the Id property from the current class
            var idProperty = entityType.GetAllMembers();
            if (idProperty != null)
            {  
                //if the id property exists, add a new id element to the @class element
                classElement.id = new id() 
                {
                    name = "Id",
                    generator = new generator() {@class = "hilo"},
                    column = {new column() {name = "Id"}}
                };
            }
        }
    }
}