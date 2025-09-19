using nauth_asp.Models;
using OtpNet;
using System.Security.Cryptography;

namespace nauth_asp.Helpers
{
    public class _2FA
    {

        public class Secret
        {
            public static string Generate()
            {
                return Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            }
        }

        public class BackupCode
        {
            public static string Generate()
            {

                var backupCode = "";
                using (var rng = new RNGCryptoServiceProvider())
                {
                    var code = new byte[4];
                    rng.GetBytes(code);
                    backupCode = BitConverter.ToString(code).Replace("-", "").ToUpper();
                }
                return backupCode;
            }
        }

        public static string GetAuthURL(DB_User user, string Secret)
        {
            return $"otpauth://totp/nauth:{user.email}?secret={Secret}&issuer=nozsa.com";
        }

        public static bool Verify(string secret, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            return totp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }

}