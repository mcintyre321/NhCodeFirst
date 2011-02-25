using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NhCodeFirst.ExampleDomain
{
    public class User
    {
        [Key, StringLength(100)]
        public virtual string Email { get; set; }
        public virtual ISet<PhotoGallery> Galleries { get; set; }
        public virtual ISet<Friendship> OutgoingFriendships { get; set; }
        public virtual ISet<Friendship> IncomingFriendships { get; set; }
    }
}