using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Authenticate the user using the AuthService
            var token = await _authService.Authenticate(request.Email, request.Password);
            if (token == null)
                return Unauthorized(); // Return 401 if authentication fails

            // Return the generated JWT token
            return Ok(new { token });
        }

     [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            // Check if the email is already registered in the database
            if (await _context.Users.AnyAsync(u => u.Person.Email == request.Email))
            {
                return BadRequest("Email is already registered.");
            }

            // Save the uploaded image to the server
            string imageUrl = null;
            if (request.Image != null)
            {
                // Define the folder path to store images
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists

                // Generate a unique file name for the image
                var fileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the image to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                // Store the relative URL of the image
                imageUrl = $"/images/{fileName}";
            }

            // Create a new Person entity with the provided details
            var person = new Person
            {
                FullName = request.FullName,
                Address = request.Address,
                Email = request.Email,
                ImageUrl = imageUrl
            };

            // Create a new User entity and associate it with the Person
            var user = new User
            {
                Username = request.Email, // Use email as the username
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hash the password
                TenantId = request.TenantId, // Associate the user with a tenant
                Person = person // Link the Person entity
            };

            // Assign roles to the user
            foreach (var roleId in request.RoleIds)
            {
                // Find the role by its ID
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return BadRequest($"Role with ID {roleId} does not exist.");
                }

                // Add the role to the user's roles
                user.UserRoles.Add(new UserRole
                {
                    User = user,
                    Role = role
                });
            }

            // Add the new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Save changes to the database

            // Return a success message
            return Ok("User registered successfully.");
        }
    }

}
public class RegisterRequest
{
    public string FullName { get; set; } // Full name of the user
    public string Address { get; set; } // Address of the user
    public string Email { get; set; } // Email of the user (used as username)
    public string Password { get; set; } // Password for the user account
    public Guid TenantId { get; set; } // Tenant ID to associate the user with a tenant
    public List<Guid> RoleIds { get; set; } = new List<Guid>(); // List of role IDs to assign to the user
    public IFormFile? Image { get; set; } // Profile image of the user
}
