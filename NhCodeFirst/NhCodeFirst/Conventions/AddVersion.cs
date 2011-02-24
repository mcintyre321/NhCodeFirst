using System;
using System.Collections.Generic;
using System.Linq;
using DependencySort;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public class AddVersion : IClassConvention, IRunAfter<CreateHiloIdIfTypeHasIntId>
    {
        public void Apply(Type type, @class @class, IEnumerable<Type> entityTypes, hibernatemapping mapping)
        {
            if (type.GetMember("Version").Any())
                @class.version = new version {name = "Version", column1 = "Version"};

        }
        
    }
}