namespace ChatApp.Web.ViewModels
{
     public class ChatMessage
     {
          public string Id { get; set; } = string.Empty;
          public string Sender { get; set; } = string.Empty;
          public string Receiver { get; set; } = string.Empty;
          public string Content { get; set; } = string.Empty;
          public DateTime Timestamp { get; set; }
          public bool IsRead { get; set; }
          public string? FileUrl { get; set; }
          public string? FileName { get; set; }
          public string? FileType { get; set; }
     }
}
