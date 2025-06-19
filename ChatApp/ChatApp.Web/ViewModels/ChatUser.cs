namespace ChatApp.Web.ViewModels
{
     public class ChatUserVM
     {
          public string Name { get; set; } = string.Empty;
          public string LastMessage { get; set; } = string.Empty;
          public string TimeAgo { get; set; } = string.Empty;
          public string AvatarUrl { get; set; } = "/images/no-profile.png";
          public bool IsOnline { get; set; }
     }
}
