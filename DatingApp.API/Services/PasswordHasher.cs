using System.Security.Cryptography;
using System.Text;

namespace DatingApp.API.Services
{
    public interface IPasswordHasher
    {
        void CreateHash(string password, out byte[] hash, out byte[] salt);
        bool Verify(string password, byte[] hash, byte[] salt);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var hmac = new HMACSHA512())
            {
                salt = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool Verify(string password, byte[] hash, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hash[i]) return false;
                }
            }

            return true;
        }
    }
}
