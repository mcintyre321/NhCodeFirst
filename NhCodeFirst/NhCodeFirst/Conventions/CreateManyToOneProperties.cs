using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class CreateManyToOneProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        private IEnumerable<Type> setTypes = new[] {typeof (ISet<>), typeof (Iesi.Collections.Generic.ISet<>)};

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping mapping)
        {
            var entityMembersOnType = type.GetAllMembers().Where(p => entityTypes.Contains(p.ReturnType())).ToArray();
            foreach (var memberInfo in entityMembersOnType.Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property))
            {
                var columnName = GetColumnName(memberInfo);
                @class.manytoone.Add(new manytoone()
                {
                    name = memberInfo.Name.Sanitise(),
                    column1 = columnName.Sanitise(),
                    access = memberInfo.Access(),
                    
                });


                //if there is a manytoone, there is probably a set on the other object...
                var potentialReciprocalProperties = memberInfo.ReturnType().GetAllMembers()
                        //... so we get the collections on the other type
                    .Where(p => setTypes.MakeGenericTypes(type).Any(t => t.IsAssignableFrom(p.ReturnType())));
                
                
                if (potentialReciprocalProperties.Count() > 1)
                {
                    var att =memberInfo.GetCustomAttributes(typeof (ManyToOneHintAttribute), false).SingleOrDefault() as ManyToOneHintAttribute;
                    if (att != null)
                    {
                        potentialReciprocalProperties =
                            potentialReciprocalProperties.Where(p => p.Name == att.CorrespondingCollectionName);
                    }
                }
                if (potentialReciprocalProperties.Count() > 1)
                    throw new Exception("Meow! There is more than 1 collection that might be the inverse! You may need to add a ManyToOneHintAttribute so we know which one to use");
                if (potentialReciprocalProperties.Count() == 1)
                {
                    var otherProp = potentialReciprocalProperties.Single();
                    var otherClassMap = mapping.@class.Single(c => c.name == memberInfo.ReturnType().AssemblyQualifiedName);

                    if (setTypes.MakeGenericTypes(type).Any(t => t.IsAssignableFrom(otherProp.ReturnType())))
                    {
                        otherClassMap.set.Add(new set()
                        {
                            name = otherProp.Name,
                            access = otherProp.Access(),
                            key = new key()
                            {
                                column1 = new column { name = columnName },
                                notnull = true,
                                foreignkey = "FK_" + memberInfo.Name + "_to_" + otherProp.Name,
                            },
                            inverse = true,
                            onetomany = new onetomany() { @class = type.AssemblyQualifiedName },
                            cascade = "all"
                        });
                    }
                }
            }
        }

        private static string GetColumnName(MemberInfo memberInfo)
        {
            var propName = memberInfo.ReturnType().Name; //suppose we have a property of type User...
            var columnName = propName + "Id";  //...we write it to a column UserId...
            if (memberInfo.Name != propName) //...but if the property is called something like CreatorUser...
            {
                columnName = memberInfo.Name.Replace(propName, "") + "_" + columnName; //...we end up with a column called Creator_UserId
            }
            return columnName;
        }
    }
    class ManyToOneHintAttribute:Attribute
    {
        public string CorrespondingCollectionName { get; private set; }

        public ManyToOneHintAttribute(string correspondingPropertyName)
        {
            CorrespondingCollectionName = correspondingPropertyName;
        }
    }
}