using NhCodeFirst.NhCodeFirst;

namespace NhCodeFirst.ExampleDomain
{
    public class Program
    {
        static void Main(params string[] args)
        {
            using (var db = new SocialNetworkDb("server=.;database=SocialNetworkDb;trusted_connection=true", DbContext.DbOption.DropAndRecreate))
            {
                var yogi = new User {Email = "yogi@jellystone.com"};
                db.Session.SaveOrUpdate(yogi);
                db.Session.Flush();

                var booboo = new User {Email = "booboo@jellystone.com"};
                db.Session.SaveOrUpdate(booboo);
                db.Session.Flush();

            }
        }
    }
}