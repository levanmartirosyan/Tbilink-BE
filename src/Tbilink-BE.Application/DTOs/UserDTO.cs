namespace Tbilink_BE.Application.DTOs
{
    public class UserDTO
    {
        public string? FirstName { get; set; } = default!;
        public string? LastName { get; set; } = default!;
        public string? UserName { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? ProfilePhotoUrl { get; set; }
        public string? Role { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}
