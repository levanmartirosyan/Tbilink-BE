using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Tbilink_BE.Application.DTOs
{
    public class FileUploadDTO
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        public string? Folder { get; set; }
    }
}
