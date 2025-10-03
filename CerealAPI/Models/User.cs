namespace CerealAPI.Models
{
    public class User
    {
        public int Id { get; set; } // Primær nøgle
        public string Username { get; set; } // Brugernavn
        public string PasswordHash { get; set; } // Hash af password
        public string Role { get; set; } // Rolle: "admin" eller "user"
    }
}
