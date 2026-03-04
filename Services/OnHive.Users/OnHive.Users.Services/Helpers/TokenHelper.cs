using OnHive.Authorization.Library.Models;
using OnHive.Core.Library.Contracts.Payments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnHive.Domains.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Users.Services.Helpers
{
    public static class TokenHelper
    {
        public static bool ValidateToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            var settings = ServiceProviderFactory.ServiceProvider?.GetService<AuthSettings>();
            var key = Encoding.ASCII.GetBytes(settings?.Secret ?? string.Empty);
            var audience = settings?.Audience;
            var issuer = settings?.Issuer;
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}