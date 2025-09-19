
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nauth_asp.DbContexts;
using nauth_asp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace nauth_asp.Helpers
{
    public class ApplyTokenProvider(IConfiguration configuration, NauthDbContext db)
    {
        public string Generate(string ActionId)
        {

            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(SHA256.Compute(configuration["JWT:secretKey"]!));

            var tokenLife = int.Parse(configuration["JWT:emailActionExpiresMinutes"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(tokenLife),
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("ActionId", ActionId ),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);

            return stringToken;
        }

        public DB_EmailAction? DecodeAndVerify(string ApplyToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                SecurityToken _token;
                var Claims = tokenHandler.ValidateToken(ApplyToken, new TokenValidationParameters
                {
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SHA256.Compute(configuration["JWT:secretKey"]!))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                }, out _token);

                string? actionId = FClaim.FindClaim(Claims, "ActionId")?.Value;

                DB_EmailAction? dbAction = db.EmailActions.Where(ev => ev.Id == long.Parse(actionId!)).AsNoTracking().FirstOrDefault();

                if (dbAction == null)
                {
                    Console.WriteLine("db action is null " + actionId);
                    return null;
                }

                if (dbAction?.CreatedAt.AddMinutes(int.Parse(configuration["JWT:emailActionExpiresMinutes"]!)) < DateTime.UtcNow)
                {
                    Console.WriteLine(int.Parse(configuration["JWT:emailActionExpiresMinutes"]!) + " invalid lifetime");
                    return null;
                }

                return dbAction;
            }
            catch (Exception e)
            {
                db.EmailActions.RemoveRange(db.EmailActions.Where(ea => ea.token == ApplyToken));

                db.SaveChanges();

                Console.WriteLine(e.Message);
                return null;
            }

        }
    }
}