using Microsoft.IdentityModel.Tokens;
using SupportMe.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SupportMe.Helpers
{
    public record JwtConfig(string Secret, string Audience, string Issuer);
    public static class JwtManager
    {
        public static string GenerateToken(JwtConfig config, User user, int expireMinutes = 1440)
        {

            //todo: set claims similar to wr-core
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
            };

            //if (user.Roles.Any())
            //{
            //    claims.AddRange(user.Roles.OrderBy(p => p.Hierarchy).Select(p => new Claim(ClaimTypes.Role, p.Name)).ToList());
            //    claims.AddRange(user.Roles.Where(p => p.Claims != null && p.Claims.Any()).SelectMany(p => p.Claims).Select(p => new Claim(p.ClaimType, p.ClaimValue)).ToList());
            //}
            var token = CreateJwt(config, claims.ToArray(), expireMinutes);

            return token;
        }
        private static string CreateJwt(JwtConfig config, Claim[] claims, int expireMinutes = 1440)
        {
            var privateKey = Convert.FromBase64String(config.Secret);
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            var credentials = new SigningCredentials(new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;
            var tokenDescriptor = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(Convert.ToInt32(expireMinutes)),
                audience: config.Audience,
                issuer: config.Audience,
                signingCredentials: credentials
            );
            var token = tokenHandler.WriteToken(tokenDescriptor);
            return token;
        }
    }
}
