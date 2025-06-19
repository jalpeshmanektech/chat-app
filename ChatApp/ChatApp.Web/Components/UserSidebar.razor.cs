using ChatApp.Web.ViewModels;
using Microsoft.AspNetCore.Components;

namespace ChatApp.Web.Components;

public partial class UserSidebar : ComponentBase
{
     [Parameter] public ChatUserVM? CurrentChatUserVM { get; set; }
     [Parameter] public List<ChatMessage> ChatMessages { get; set; } = new();

     protected List<ChatMessage> MediaItems => ChatMessages
         .Where(m => !string.IsNullOrEmpty(m.ImageUrl) || !string.IsNullOrEmpty(m.FileUrl))
         .ToList();
} 