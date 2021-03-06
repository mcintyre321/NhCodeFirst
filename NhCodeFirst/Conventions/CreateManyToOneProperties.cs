using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencySort;
using NHibernate.Mapping;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.Conventions
{
    public class CreateManyToOneProperties : IClassConvention, IRunAfter<CreateNonCompositeIdentity>, IRunAfter<AddVersion>
    {
        private IEnumerable<Type> setTypes = new[] { typeof(ISet<>), typeof(Iesi.Collections.Generic.ISet<>) };

        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            var entityMembersOnType = type.GetFieldsAndProperties().Where(p => entityTypes.Contains(p.ReturnType())).ToArray();
            foreach (var memberInfo in entityMembersOnType)
            {
                var prefix = ColumnNamePrefix(memberInfo);
                var entityClassElement = memberInfo.ReturnType().ClassElement(hbm);
                var manyToOne = new manytoone()
                                    {
                                        name = memberInfo.Name.Sanitise(),
                                        column = entityClassElement.id.column.Copy()
                                            .Each(c => c.SetName(prefix + c.GetName()))
                                            .Each(c => c.index = null)
                                            .Each(c => c.notnull = !memberInfo.IsNullable()).ToList(),
                                        access = memberInfo.Access(),
                                        
                                    };
                manyToOne.foreignkey = "FK_" + @class.table.Trim('[', ']') + "_" +
                                       string.Join("_", manyToOne.column.Select(c => c.name.Trim("[]".ToCharArray()))) +
                                       "_to_" + memberInfo.ReturnType().Name.Sanitise();
                UniqueAttribute.SetUniqueProperties(memberInfo, manyToOne);

                @class.manytoone.Add(manyToOne);


                //if there is a manytoone, there is probably a set on the other object...
                var potentialCorrespondingCollections = memberInfo.ReturnType().GetAllMembers()
                    //... so we get the collections on the other type
                    .Where(p => setTypes.MakeGenericTypes(type).Any(t => t.IsAssignableFrom(p.ReturnType())));


                if (potentialCorrespondingCollections.Count() > 1)
                {
                    var att = memberInfo.GetCustomAttributes(typeof(ManyToOneHintAttribute), false).SingleOrDefault() as ManyToOneHintAttribute;
                    if (att != null)
                    {
                        potentialCorrespondingCollections =
                            potentialCorrespondingCollections.Where(p => p.Name == att.CorrespondingCollectionName);
                    }
                }
                if (potentialCorrespondingCollections.Count() > 1)
                    throw new Exception("Meow! There is more than 1 collection that might be the inverse! You may need to add a ManyToOneHintAttribute so we know which one to use");
                if (potentialCorrespondingCollections.Count() == 1)
                {
                    var correspondingCollection = potentialCorrespondingCollections.Single();

                    if (setTypes.MakeGenericTypes(type).Any(t => t.IsAssignableFrom(correspondingCollection.ReturnType())))
                    {

                        var set = new set()
                                      {
                                          name = correspondingCollection.Name,
                                          access = correspondingCollection.Access(),
                                          key = new key()
                                                    {
                                                        column1 = manyToOne.column.Single().name,
                                                        foreignkey = "FK_" + @class.table.Trim('[', ']') + "_" + string.Join("_", manyToOne.column.Select(c => c.name.Trim("[]".ToCharArray()))) + "_to_" + memberInfo.ReturnType().ClassElement(hbm).table,
                                                        notnull = false// so inverse works!memberInfo.IsNullable(),
                                                    },
                                          inverse = true,
                                          onetomany = new onetomany() { @class = type.AssemblyQualifiedName },
                                          cascade = "all",

                                      };
                        manyToOne.column.Each(c => c.notnull = false).ToArray();
                        var otherClassMap = memberInfo.ReturnType().ClassElement(hbm);

                        otherClassMap.set.Add(set);

                    }
                }
            }
        }

        private static string ColumnNamePrefix(MemberInfo memberInfo)
        {
            var propName = memberInfo.ReturnType().Name.Sanitise(); //suppose we have a property of type User...
            var columnName = propName;  //...we write it to a column UserId...
            if (memberInfo.Name != propName) //...but if the property is called something like CreatorUser...
            {
                columnName = memberInfo.Name.Replace(propName, "") + "_" + columnName; //...we end up with a column called Creator_UserId
            }
            return columnName;
        }
        public IEnumerable<IAuxiliaryDatabaseObject> AuxDbObjects()
        {
            yield break;
        }
    }
    class ManyToOneHintAttribute : Attribute
    {
        public string CorrespondingCollectionName { get; private set; }

        public ManyToOneHintAttribute(string correspondingPropertyName)
        {
            CorrespondingCollectionName = correspondingPropertyName;
        }
    }
}