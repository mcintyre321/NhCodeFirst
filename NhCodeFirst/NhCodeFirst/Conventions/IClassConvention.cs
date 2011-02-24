using System;
using System.Collections.Generic;
using urn.nhibernate.mapping.Item2.Item2;

namespace NhCodeFirst.NhCodeFirst.Conventions
{
    public interface IClassConvention
    {
        void Apply(Type entityType, @class classElement, IEnumerable<Type> entityTypes, hibernatemapping mapping);
    }
}