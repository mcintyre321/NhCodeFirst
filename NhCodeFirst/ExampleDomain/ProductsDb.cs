using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NhCodeFirst.NhCodeFirst;

namespace NhCodeFirst.ExampleDomain
{
    class SocialNetworkDb : DbContext
    {
        public SocialNetworkDb(string connectionString)
            : base(connectionString)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Friendship> Friendship { get; set; }
        public DbSet<PhotoGallery> PhotoGallery { get; set; }

    }

    internal class PhotoGallery
    {
    }

    internal class Friendship
    {
        public int Id { get; set; }
        enum FreindshipResponse
        {
            Rejected,
            Accepted
        }
        [Unique]
        User Friender { get; set; }
        [Unique]
        User Friendee { get; set; }
        DateTime Requested { get; set; }
        DateTime? Responded { get; set; }
        FreindshipResponse Response { get; set; }
    }

    internal class User
    {
    }
}
