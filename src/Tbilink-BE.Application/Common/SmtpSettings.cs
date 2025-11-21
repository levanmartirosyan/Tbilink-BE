namespace Tbilink_BE.Models
{
    public class SmtpSettings
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public required string User { get; set; }
        public required string Password { get; set; }
    }

}
