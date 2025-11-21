using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ServiceResponse<RegisterResultDTO>> Register(RegisterDTO registerDTO);
        public Task<ServiceResponse<LoginResultDto>> Login(LoginDTO loginDTO);
        public Task<ServiceResponse<string>> SendtVerificationCode(SendVerificationCodeDTO sendVerificationCodeDTO);
        public Task<ServiceResponse<EmailVerificationResultDTO>> VerifyEmail(string email, string code);
        public Task<ServiceResponse<CreateNewPasswordDTO>> CreateNewPassword(CreateNewPasswordDTO createNewPasswordDTO);
    }
}
