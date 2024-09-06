using Microsoft.IdentityModel.Tokens;
using Spike.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpikeApi.Common
{
    public class JwtTokenFactory : IJwtTokenFactory
    {
        private readonly IConfiguration _configuration;

        public JwtTokenFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string email, string roleName)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Role, roleName)                
            };

            var token = new JwtSecurityToken(
                            issuer: _configuration["JwtSettings:Issuer"].ToString(),
                            audience: _configuration["JwtSettings:Audience"].ToString(),
                            claims: claims,
                            notBefore:DateTime.Now.AddMinutes(-1),
                            expires: DateTime.Now.AddMinutes(5),
                            signingCredentials: credentials
                        );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public string ValidateToken(string token)
        {           
            if (!token.StartsWith("Bearer"))
                return CommonConstants.INVALID_JWT_TOKEN;

            var plainToken = token.Split("Bearer");


            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(plainToken[1]?.Trim(), new TokenValidationParameters
                {
                    ValidIssuer = _configuration["JwtSettings:Issuer"].ToString(),
                    ValidAudience = _configuration["JwtSettings:Audience"].ToString(),
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"].ToString())),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var tokenValidated = (JwtSecurityToken)validatedToken;
                var role = tokenValidated.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;
                
                if(role == null || (role != CommonConstants.ADMIN && role != CommonConstants.SUPERADMIN)) 
                {
                    return CommonConstants.UNAUTHORIZED_USER;
                }
            }
            catch (SecurityTokenValidationException ex)
            {
                Console.WriteLine(ex.StackTrace);
                return CommonConstants.SOME_ERROR_OCCURRED;
            }

            return CommonConstants.SUCCESSFUL;
        }        
    }
}
