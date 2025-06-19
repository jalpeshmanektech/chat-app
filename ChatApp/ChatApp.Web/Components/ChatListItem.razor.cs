using Microsoft.AspNetCore.Components;

namespace ChatApp.Web.Components;

public partial class ChatListItem : ComponentBase
{
     [Parameter] public string Name { get; set; } = string.Empty;
     [Parameter] public string LastMessage { get; set; } = string.Empty;
     [Parameter] public string TimeAgo { get; set; } = string.Empty;
     [Parameter] public string AvatarUrl { get; set; }
     [Parameter] public bool IsOnline { get; set; }
     [Parameter] public EventCallback<string> OnUserSelected { get; set; }

     private Task HandleClick() => OnUserSelected.InvokeAsync(Name);
     protected override void OnInitialized()
     {
          AvatarUrl = "/images/no-profile.png";
     }
}