namespace Api_proj.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Mandatory field with no default fallback //
        public string RoleName { get; set; } = string.Empty;
    }
}//