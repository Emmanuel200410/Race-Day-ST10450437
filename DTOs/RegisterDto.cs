namespace Api_proj.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; } // 1 for Organiser, 2 for Participant (matching your Role table)
    }
}