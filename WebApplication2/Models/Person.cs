namespace WebApplication2.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }

        public ICollection<UserRole> PersonRoles { get; set; } = new List<UserRole>();


        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
