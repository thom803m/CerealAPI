namespace CerealAPI.DTOs
{
    // DTO til login – bruges som input fra klienten
    public class LoginRequest
    {
        public string Username { get; set; } // Brugernavn
        public string Password { get; set; } // Password i klartekst (hashes på server)
    }
}