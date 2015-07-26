using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Serpent_Theater_Server.FTP
{
    public static class UserStore
    {
        private static readonly List<User> Users;

        static UserStore()
        {
            Users = new List<User>();

            var serializer = new XmlSerializer(Users.GetType(), new XmlRootAttribute("Users"));

            if (File.Exists("users.xml"))
            {
                Users = serializer.Deserialize(new StreamReader("users.xml")) as List<User>;
            }
            else
            {
                Users.Add(new User {
                    Username = "rick",
                    Password = "test",
                    HomeDir = "C:\\Utils"
                });

                using (var w = new StreamWriter("users.xml"))
                {
                    serializer.Serialize(w, Users);
                }
            }
        }

        public static User Validate(string username, string password)
        {
            var user = (from u in Users where u.Username == username && u.Password == password select u).SingleOrDefault();

            return user;
        }
    }
}
