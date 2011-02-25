using System;
using System.Collections.Generic;
using NhCodeFirst.NhCodeFirst;

namespace NhCodeFirst.ExampleDomain
{
    class SocialNetworkDb : DbContext
    {
        public SocialNetworkDb(string connectionString, DbOption dbOption) : base(connectionString, dbOption)
        {
        }

        public override IEnumerable<Type> GetEntityTypes()
        {
            yield return typeof (User);
            yield return typeof (Friendship);
            yield return typeof (PhotoGallery);
        }
    }
}