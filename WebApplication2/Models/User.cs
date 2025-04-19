namespace WebApplication2.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public Person Person { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public string Password { get; internal set; }
    }
}
