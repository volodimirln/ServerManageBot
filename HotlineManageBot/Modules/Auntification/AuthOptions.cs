using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotlineManageBot.Modules.Auntification
{
    public class AuthOptions
    {
        public AuthOptions() 
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();
            ISSUER = config["JwtIssue"];
            AUDIENCE = config["JwtAudience"];
            KEY = config["JwtKey"];

        }
         string ISSUER { set; get; } 
         string AUDIENCE { set; get; }
          string KEY { set; get; }
        public  SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        public string GetName(string token)
        {
            string secret = KEY;
            var key = Encoding.ASCII.GetBytes(secret);
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            return claims.Identity.Name;
        }

        public string CreateJWT(string username)
        {

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
            var jwt = new JwtSecurityToken(
                issuer: ISSUER,
                audience: AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(365)),
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
