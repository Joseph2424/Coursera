namespace UserManagementAPI.Models
{
    public class User
    {
        public int Id { get; set; }          // Unique identifier
        public required string Name { get; set; }     // User's name
        public required string Email { get; set; }    // User's email
    }
}
