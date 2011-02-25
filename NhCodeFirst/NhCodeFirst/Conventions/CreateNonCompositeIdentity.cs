using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateNonCompositeIdentity : IClassConvention
    {
        public void Apply(Type entityType, @class classElement, IEnumerable<Type> entityTypes, hibernatemapping mapping)
        {
            //use reflection to get the Id property from the current class
            var idProperty = entityType.GetFieldsAndProperties()
                .Where(e => e.Name.ToLower() == "id" || e.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .SingleOrDefault();
            if (idProperty != null)
            {
                var idType = idProperty.ReturnType();
                //if the id property exists, add a new id element to the @class element
                classElement.id = new id() 
                {
                    name = idProperty.Name.Sanitise(),
                    column = { new column() { name = idProperty.Name.Sanitise() } }
                };
                if (CanUseHiloGenerator(idType)) //if is integer of some kind
                {
                    classElement.id.generator = new generator() {@class = "hilo"};
                }
                if (idType == typeof(string))
                {
                    var stringLengthAttribute = idProperty.GetCustomAttributes(true).OfType<StringLengthAttribute>().SingleOrDefault();
                    string maxLength = stringLengthAttribute == null ? "MAX" : stringLengthAttribute.MaximumLength.ToString();
                    classElement.id.column.Single().sqltype = "NVARCHAR(" + maxLength + ")";
                }
            }
        }

        private bool CanUseHiloGenerator(Type idType)
        {
            return new[]{typeof(int), typeof(long)}.Contains(idType);
        }
    }
}