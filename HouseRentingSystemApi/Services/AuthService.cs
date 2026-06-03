using HouseRentingSystemApi.Data.Entities;
using HouseRentingSystemApi.Models.Authorization;
using HouseRentingSystemApi.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HouseRentingSystemApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        // Позволени роли при регистрация
        private static readonly HashSet<string> AllowedRoles = new() { "Agent", "Client" };

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return PopulateResult(400, null, "Потребителят не съществува.");
            }

            var passwordOk = await userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordOk)
            {
                return PopulateResult(400, null, "Грешна парола.");
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return PopulateResult(200, token, "Успешен вход.");
        }

        public async Task<AuthResult> RegisterAsync(Register model)
        {
            var role = model.Role ?? "Client";
            if (!AllowedRoles.Contains(role))
            {
                return PopulateResult(400, null, $"Невалидна роля '{role}'. Позволени: Agent, Client.");
            }

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return PopulateResult(400, null, "Потребителят вече съществува.");
            }

            var newUser = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Username
            };

            var result = await userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return PopulateResult(400, null, errors);
            }

            // Ролите се създават при старт на приложението
            await userManager.AddToRoleAsync(newUser, role);

            return PopulateResult(200, null, $"Потребителят е регистриран успешно с роля '{role}'.");
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"]!;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            // Ролите като claims - ключово за [Authorize(Roles="...")]
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                int.Parse(jwtSection["ExpiresInMinutes"]!)
            );

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static AuthResult PopulateResult(int code, string? token = null, params string[] messages)
        {
            return new AuthResult
            {
                Code = code,
                Massage = string.Join(Environment.NewLine, messages),
                Token = token ?? string.Empty
            };
        }
    }
}
