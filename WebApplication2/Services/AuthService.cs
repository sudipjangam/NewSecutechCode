using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApplication2.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication2.Data;

namespace WebApplication2.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> Authenticate(string username, string password)
        {
            // Step 1: Retrieve the user from the database based on the provided username and password.
            // Include the user's roles to generate claims for the JWT token.
            var user = await _context.Users
                .Include(u => u.UserRoles) // Include the UserRoles relationship
                .ThenInclude(ur => ur.Role) // Include the Role relationship
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            // Step 2: If the user is not found or the credentials are invalid, return null.
            if (user == null) return null;

            // Step 3: Create a list of claims for the JWT token.
            // Claims are used to store information about the user in the token.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Add the user's unique ID
                new Claim(ClaimTypes.Name, user.Username) // Add the user's username
            };

            // Step 4: Add role claims for the user based on their assigned roles.
            claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

            // Step 5: Generate a symmetric security key using the JWT key from the configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            // Step 6: Create signing credentials using the security key and HMAC-SHA256 algorithm.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 7: Create the JWT token with the claims, expiration time, and signing credentials.
            var token = new JwtSecurityToken(
                claims: claims, // Add the claims to the token
                expires: DateTime.Now.AddHours(1), // Set the token to expire in 1 hour
                signingCredentials: creds // Add the signing credentials
            );

            // Step 8: Return the serialized JWT token as a string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
