using Microsoft.AspNetCore.Http;

namespace Tbilink_BE.Services.Interfaces
{
public interface IFileUploadService
{
    Task<string> UploadFile(IFormFile file, string folder);
}
}
