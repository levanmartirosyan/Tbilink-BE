using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IStorageService
    {
        Task<ServiceResponse<UploadPublicResultDTO>> UploadPublicAsync(Stream data, string fileName, string folder = "");
        Task<ServiceResponse<UploadPrivateResultDTO>> UploadPrivateAsync(Stream data, string fileName, string folder = "");
        Task<ServiceResponse<object>> DeleteObjectAsync(string bucket, string path);
        Task<ServiceResponse<string>> CreateSignedUrlAsync(string bucket, string path, bool preview = true);
    }

}
