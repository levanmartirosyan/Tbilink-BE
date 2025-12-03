using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tbilink_BE.Application.Common;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class StorageService : IStorageService
    {
        private readonly HttpClient _http;
        private readonly SupabaseOptions _opts;

        public StorageService(IHttpClientFactory httpFactory, IOptions<SupabaseOptions> opts)
        {
            _http = httpFactory.CreateClient("supabase-storage");
            _opts = opts.Value;

            if (string.IsNullOrEmpty(_opts.ServiceRoleKey))
                throw new InvalidOperationException("Supabase service role key is not configured.");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opts.ServiceRoleKey);
        }

        private string BuildObjectUrl(string bucket, string path) =>
            $"{_opts.Url}/storage/v1/object/{bucket}/{Uri.EscapeDataString(path)}";

        private string BuildPublicObjectUrl(string bucket, string path) =>
            $"{_opts.Url}/storage/v1/object/public/{bucket}/{Uri.EscapeDataString(path)}";

        public async Task<ServiceResponse<UploadPublicResultDTO>> UploadPublicAsync(Stream data, string fileName, string folder = "")
        {
            try
            {
                var bucket = _opts.Buckets.PublicBucket;
                var path = string.IsNullOrWhiteSpace(folder) ? $"{Guid.NewGuid()}-{fileName}" : $"{folder}/{Guid.NewGuid()}-{fileName}";
                var url = BuildObjectUrl(bucket, path);

                using var content = new StreamContent(data);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var resp = await _http.PostAsync(url + "?upsert=true", content);
                if (!resp.IsSuccessStatusCode)
                    return ServiceResponse<UploadPublicResultDTO>.Fail(null, await resp.Content.ReadAsStringAsync(), 500);

                var publicUrl = BuildPublicObjectUrl(bucket, path);
                var result = new UploadPublicResultDTO { Path = path, PublicUrl = publicUrl };
                return ServiceResponse<UploadPublicResultDTO>.Success(result, "File uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<UploadPublicResultDTO>.Fail(null, ex.Message, 500);
            }
        }

        public async Task<ServiceResponse<UploadPrivateResultDTO>> UploadPrivateAsync(Stream data, string fileName, string folder = "")
        {
            try
            {
                var bucket = _opts.Buckets.PrivateBucket;
                var path = string.IsNullOrWhiteSpace(folder) ? $"chats/{Guid.NewGuid()}-{fileName}" : $"{folder}/{Guid.NewGuid()}-{fileName}";
                var url = BuildObjectUrl(bucket, path);

                using var content = new StreamContent(data);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var resp = await _http.PostAsync(url + "?upsert=true", content);
                if (!resp.IsSuccessStatusCode)
                    return ServiceResponse<UploadPrivateResultDTO>.Fail(null, await resp.Content.ReadAsStringAsync(), 500);

                var result = new UploadPrivateResultDTO { Path = path };
                return ServiceResponse<UploadPrivateResultDTO>.Success(result, "File uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<UploadPrivateResultDTO>.Fail(null, ex.Message, 500);
            }
        }

        public async Task<ServiceResponse<object>> DeleteObjectAsync(string bucket, string path)
        {
            try
            {
                var url = BuildObjectUrl(bucket, path);
                var resp = await _http.DeleteAsync(url);
                if (!resp.IsSuccessStatusCode)
                    return ServiceResponse<object>.Fail(null, await resp.Content.ReadAsStringAsync(), 500);

                return ServiceResponse<object>.Success(null, "Object deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Fail(null, ex.Message, 500);
            }
        }
        public async Task<ServiceResponse<string>> CreateSignedUrlAsync(string bucket, string path, bool preview = true)
        {
            try
            {
                var encodedPath = string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
                var url = $"{_opts.Url}/storage/v1/object/sign/{bucket}/{encodedPath}";
                var body = new { expiresIn = 1296000 }; 

                var resp = await _http.PostAsJsonAsync(url, body); 
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return ServiceResponse<string>.Fail(null, json, 500);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("signedURL", out var v))
                {
                    var signedUrl = v.GetString()!;

                    if (preview)
                        signedUrl += signedUrl.Contains("?") ? "&download=false" : "?download=false";

                    return ServiceResponse<string>.Success(signedUrl, "Signed URL created successfully");
                }

                return ServiceResponse<string>.Success(json, "Signed URL created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, ex.Message, 500);
            }
        }

    }

}
