namespace CerealAPI.DTOs
{
    // DTO til login-svar, der returner til klienten efter succesfuld login
    public class LoginResponse
    {
        public string Token { get; set; } // JWT token til autorisation
        public DateTime Expires { get; set; } // Token udløbstid
    }
}
