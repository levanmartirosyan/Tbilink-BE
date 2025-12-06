namespace Tbilink_BE.Application.DTOs
{
    public class ChatDTO
    {
        public string GroupName { get; set; }
        public List<UserDTO> Participants { get; set; } = new List<UserDTO>();
        public MessageDTO? LastMessage { get; set; }
        public DateTime? LastActivity { get; set; }
        public int UnreadCount { get; set; }
    }
}
