using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace nauth_asp.Helpers
{
    public static class Argon2idHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 3;
        private const int MemorySizeKB = 65536;
        private const int Parallelism = 2;

        public static string Hash(string password)
        {
            byte[] salt = RandomBytes(SaltSize);
            byte[] hash = Argon2id(password, salt, Iterations, MemorySizeKB, Parallelism, HashSize);

            string saltB64 = ToB64NoPad(salt);
            string hashB64 = ToB64NoPad(hash);
            return $"$argon2id$v=19$m={MemorySizeKB},t={Iterations},p={Parallelism}${saltB64}${hashB64}";
        }

        public static bool Verify(string password, string phc)
        {

            var parts = phc.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5 || parts[0] != "argon2id") return false;
            if (parts[1] != "v=19") return false;


            var parms = parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Split('='))
                                .ToDictionary(k => k[0], v => v[1]);

            int m = int.Parse(parms["m"]);
            int t = int.Parse(parms["t"]);
            int p = int.Parse(parms["p"]);

            byte[] salt = FromB64NoPad(parts[3]);
            byte[] expected = FromB64NoPad(parts[4]);

            byte[] actual = Argon2id(password, salt, t, m, p, expected.Length);


            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static byte[] Argon2id(string password, byte[] salt, int t, int mKB, int p, int outLen)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                Iterations = t,
                MemorySize = mKB,
                DegreeOfParallelism = p
            };
            return argon2.GetBytes(outLen);
        }

        private static byte[] RandomBytes(int size)
        {
            byte[] b = new byte[size];
            RandomNumberGenerator.Fill(b);
            return b;
        }

        private static string ToB64NoPad(byte[] data) =>
            Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromB64NoPad(string s)
        {
            string padded = s.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}