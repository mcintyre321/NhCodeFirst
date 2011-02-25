using NhCodeFirst.NhCodeFirst;

namespace NhCodeFirst.ExampleDomain
{
    public class Program
    {
        static void Main(params string[] args)
        {
            using (var db = new SocialNetworkDb("server=.;database=SocialNetworkDb;trusted_connection=true", DbContext.DbOption.DropAndRecreate))
            {
                var user = new User {Email = "mark@thefacebook.com"};
                db.Session.SaveOrUpdate(user);
                db.Session.Flush();
            }
            using (var db = new SocialNetworkDb("server=.;database=SocialNetworkDb;trusted_connection=true", DbContext.DbOption.DropAndRecreate))
            {
                var user = new User { Email = "mark@thefacebook.com" };
                db.Session.SaveOrUpdate(user);
                db.Session.Flush();
            }
        }
    }
}