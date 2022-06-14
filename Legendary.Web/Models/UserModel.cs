using System;
namespace Legendary.Web.Models
{
    public class UserModel
    {
        public string UserName { get; private set; }

        public string Password { get; private set; }

        public UserModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }
    }
}
