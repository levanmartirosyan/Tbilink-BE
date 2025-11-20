using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Services.Interfaces;

public interface ITokenProvider
{
    (string, DateTime) CreateAccessToken(UserDTO userDTO);
    (string, DateTime) CreateRefreshToken();
}
