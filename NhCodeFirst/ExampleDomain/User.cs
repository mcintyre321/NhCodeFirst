using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate.Linq;

namespace NhCodeFirst.ExampleDomain
{
    public class User
    {
        [Key, StringLength(100)]
        public virtual string Email { get; set; }
        public virtual ISet<PhotoGallery> Galleries { get; set; }
        public virtual ISet<Friendship> OutgoingFriendships { get; set; }
        public virtual ISet<Friendship> IncomingFriendships { get; set; }
        public virtual IEnumerable<User> Friends
        {
            get
            {
                return IncomingFriendships
                    .Where(c => c.Response == Friendship.FriendshipResponse.Accepted)
                    .Select(f => f.Origin);
            }
        }

         
    }
}