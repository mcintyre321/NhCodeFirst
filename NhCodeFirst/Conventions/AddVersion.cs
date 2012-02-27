using System;
using System.Collections.Generic;
using System.Linq;
using DependencySort;
using NHibernate.Mapping;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.Conventions
{
    public class AddVersion : IClassConvention, IRunAfter<CreateNonCompositeIdentity>
    {
        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping hbm)
        {
            if (type.GetMember("Version").Any())
                @class.version = new version {name = "Version", column1 = "Version"};

        }

        public IEnumerable<IAuxiliaryDatabaseObject> AuxDbObjects()
        {
            yield break;
        }
    }
}