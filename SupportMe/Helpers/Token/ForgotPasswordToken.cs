using Microsoft.Extensions.Configuration;
using SupportMe.Helpers;
using System;
using System.Globalization;
using System.Text;

namespace Core.BusinessLogic
{
    public class ForgotPasswordToken
    {
        public string Email { get; set; }
        public DateTime ExpirationDateUTC { get; set; }

        public ForgotPasswordToken()
        {
        }

        public ForgotPasswordToken(string email, TimeSpan validityPeriod)
        {
            this.Email = email;
            this.ExpirationDateUTC = DateTime.UtcNow.Add(validityPeriod);
        }

        public string Encrypt(IConfiguration configuration)
        {
            return Crypto.EncryptSymmetricKey(getKey(configuration), serialize(this));
        }

        public static ForgotPasswordToken Decrypt(string encryptedToken, IConfiguration configuration)
        {
            string decrypted = Crypto.DecryptSymmetricKey(getKey(configuration), encryptedToken);
            return deserialize(decrypted);
        }

        public bool HasExpired()
        {
            return DateTime.UtcNow > this.ExpirationDateUTC;
        }

        private static string serialize(ForgotPasswordToken token)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(token.Email);
            sb.Append("|");
            sb.Append(token.ExpirationDateUTC.ToString("yyyyMMddHHmmss"));
            return sb.ToString();
        }

        private static ForgotPasswordToken deserialize(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            try
            {
                var parts = value.Split('|');
                if (parts.Length != 2) return null;

                var email = parts[0];

                if (!DateTime.TryParseExact(parts[1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
                {
                    return null;
                }

                return new ForgotPasswordToken
                {
                    Email = email,
                    ExpirationDateUTC = expirationDate
                };
            }
            catch
            {
                return null;
            }
        }

        private static string getKey(IConfiguration configuration)
        {
            return configuration.GetValue<string>("SUPPORTME_SECRET");
        }
    }
}
