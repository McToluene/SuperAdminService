using System.Security.Cryptography;

namespace SuperAdmin.Service.Helpers
{
    public static class PasswordGenerator
    {
        private const string LOWERCASE_CHARS = "abcdefghijklmnopqrstuvwxyz";
        private const string UPPERCASE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DIGIT_CHARS = "0123456789";
        private const string SPECIAL_CHARS = "!@#$%^&*()_-+=<>?";

        public static string GetRandomPassword(int length)
        {
            string validChars = LOWERCASE_CHARS + UPPERCASE_CHARS + DIGIT_CHARS + SPECIAL_CHARS;
            char[] password = new char[length];

            using (RNGCryptoServiceProvider rng = new())
            {
                for (int i = 0; i < length; i++)
                {
                    byte[] randomBytes = new byte[1];
                    rng.GetBytes(randomBytes);
                    int index = randomBytes[0] % validChars.Length;
                    password[i] = validChars[index];
                }
            }

            return new string(password);
        }
    }
}
