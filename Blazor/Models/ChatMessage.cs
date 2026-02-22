namespace Blazor.Models
{
    /// <summary>
    /// Model for chat beskeder i ticket systemet
    /// </summary>
    public class ChatMessage
    {
        public string Id { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }
}
