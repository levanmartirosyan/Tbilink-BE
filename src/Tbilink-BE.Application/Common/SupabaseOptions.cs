namespace Tbilink_BE.Application.Common
{
    public class SupabaseOptions
    {
        public string Url { get; set; } = string.Empty; 
        public string ServiceRoleKey { get; set; } = string.Empty; 
        public BucketOptions Buckets { get; set; } = new BucketOptions();
    }
}
