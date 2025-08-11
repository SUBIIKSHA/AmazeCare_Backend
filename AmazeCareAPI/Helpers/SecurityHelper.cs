using AmazeCareAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace AmazeCareAPI.Helpers
{
    public static class SecurityHelper
    {
        public static  User PopulateUserObject(string username, string email, string password)
        {
            HMACSHA256 hmac = new HMACSHA256();

            var user = new User
            {
                UserName = username,
                Email = email,
                HashKey = hmac.Key,
                Password = hmac.ComputeHash(Encoding.UTF8.GetBytes(password))
            };

            return user;
        }
    }
}
