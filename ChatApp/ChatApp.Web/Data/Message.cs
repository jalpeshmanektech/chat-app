namespace ChatApp.Web.Data
{
     public class Message
     {
          public int Id { get; set; }
          public string SenderId { get; set; } = string.Empty;
          public string ReceiverId { get; set; } = string.Empty;
          public string Content { get; set; } = string.Empty;
          public DateTime Timestamp { get; set; } = DateTime.UtcNow;
          public bool IsRead { get; set; } = false;
          public string? FileUrl { get; set; }
          public string? FileName { get; set; }
          public string? FileType { get; set; } // image, gif, pdf, video, doc, etc.
     }
}
