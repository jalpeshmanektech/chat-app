using Microsoft.AspNetCore.Components;

namespace ChatApp.Web.Components;

public partial class ChatListItem : ComponentBase
{
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string LastMessage { get; set; } = string.Empty;
    [Parameter] public string TimeAgo { get; set; } = string.Empty;
    [Parameter] public string AvatarUrl { get; set; } = "/images/avatar-placeholder.png";
    [Parameter] public bool IsOnline { get; set; }
} 