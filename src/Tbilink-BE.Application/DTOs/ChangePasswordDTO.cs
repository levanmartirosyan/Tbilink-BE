namespace Tbilink_BE.Application.DTOs
{
    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string RepeatNewPassword { get; set; } = string.Empty;
    }
}
