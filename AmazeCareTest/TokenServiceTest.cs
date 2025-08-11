using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPI.Tests
{
    public class TokenServiceTest
    {
        private ITokenService _tokenService;
        private string _secretKey;

        [SetUp]
        public void Setup()
        {
            _secretKey = "ThisIsASecretKeyForUnitTestingAmazecare";
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Tokens:JWT"] = _secretKey
                })
                .Build();

            _tokenService = new TokenService(config);
        }

        [Test]
        public async Task GenerateToken_ShouldReturnValidToken_WithCorrectClaims()
        {
            var user = new TokenUser
            {
                Username = "TestUser",
                Role = "Admin"
            };

            var token = await _tokenService.GenerateToken(user);

            Assert.That(token, Is.Not.Null.And.Not.Empty);

            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var validations = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            var principal = handler.ValidateToken(token, validations, out var validatedToken);

            Assert.That(principal?.Identity?.Name, Is.EqualTo("TestUser"));
            Assert.That(principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value, Is.EqualTo("Admin"));
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
