using System;
using System.Linq;
using NhCodeFirst.NhCodeFirst.Conventions;

namespace NhCodeFirst.ExampleDomain
{
    public class Friendship
    {
        public virtual int Id { get; set; }

        //for NHibernate proxy
        protected Friendship()
        {
        }

        public enum FriendshipResponse
        {
            Rejected,
            Accepted
        }

        //the unique key is here to stop duplicate records being entered
        [Unique][ManyToOneHint("OutgoingFriendships")]
        public virtual User Origin { get; set; }
        [Unique][ManyToOneHint("IncomingFriendships")]
        public virtual User Target { get; set; }

        public virtual DateTime Requested { get; private set; } //properties can be private (and DateTimes)
        DateTimeOffset? _responded; //or even private fields - still gets mapped to a column called Responded though. Check the jazzy DateTimeOffset type!
        public virtual FriendshipResponse Response { get; private set; } //or can even be enums. Mapped as strings by default
        
        
        //its a great idea to keep your entities hard by using setter methods
        public virtual void Respond(FriendshipResponse response)
        {
            //good idea to check that the current user is the friendee here... but thats another story...
            _responded = DateTime.Now;
            Response = response;
        }
    }
}
