namespace Tbilink_BE.Application.DTOs
{
    public class LoginResultDto
    {
        public AuthTokensDTO Tokens { get; set; } = default!;
        public UserDTO Data { get; set; } = default!;
    }
}
