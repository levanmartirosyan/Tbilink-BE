using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tbilink_BE.Application.Common;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi.Controllers
{
    [Route("api/storage")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storage;
        private readonly SupabaseOptions _opts;

        public StorageController(IStorageService storage, IOptions<SupabaseOptions> options)
        {
            _storage = storage;
            _opts = options.Value;
        }

        [Authorize]
        [HttpPost("upload/public")]
        public async Task<ActionResult<ServiceResponse<UploadPublicResultDTO>>> UploadPublic([FromForm] FileUploadDTO dto)
        {
            using var stream = dto.File.OpenReadStream();
            var response = await _storage.UploadPublicAsync(stream, dto.File.FileName, dto.Folder ?? "");
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpPost("upload/private")]
        public async Task<ActionResult<ServiceResponse<UploadPrivateResultDTO>>> UploadPrivate([FromForm] FileUploadDTO dto)
        {
            using var stream = dto.File.OpenReadStream();
            var response = await _storage.UploadPrivateAsync(stream, dto.File.FileName, dto.Folder ?? "");
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpDelete("object")]
        public async Task<ActionResult<ServiceResponse<object>>> Delete([FromQuery] string bucket, [FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(path))
                return ServiceResponse<object>.Fail(null, "Bucket and path required", 400);

            var response = await _storage.DeleteObjectAsync(bucket, path);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet("signed-url")]
        public async Task<ActionResult<ServiceResponse<string>>> GetSignedUrl([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return ServiceResponse<string>.Fail(null, "Path required", 400);

            var bucket = _opts.Buckets.PrivateBucket;
            var response = await _storage.CreateSignedUrlAsync(bucket, path);

            if (!response.IsSuccess)
                return StatusCode(response.StatusCode, response);

            return ServiceResponse<string>.Success(response.Data!, response.Message);
        }
    }
}
