using ChatApp.Web.Data;
using ChatApp.Web.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Web.Components;

public partial class ChatList : ComponentBase
{
     [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
     [Inject] private ApplicationDbContext DbContext { get; set; } = default!;
     [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
     [Parameter] public EventCallback<string> OnUserSelected { get; set; }

     protected List<ChatUserVM> users = new();

     private string searchTerm = string.Empty;
     private IEnumerable<ChatUserVM> FilteredUsers =>
         string.IsNullOrWhiteSpace(searchTerm) ? users  : users.Where(u =>
                 u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 u.LastMessage.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

     protected override async Task OnInitializedAsync()
     {
          var authState = await AuthProvider.GetAuthenticationStateAsync();
          var currentUser = authState.User.Identity?.Name;
          var dbUsers = UserManager.Users.ToList();
          users = new List<ChatUserVM>();

          foreach (var u in dbUsers)
          {
               if (u.UserName == currentUser)
                    continue;

               var lastMsg = await DbContext.Messages
                   .Where(m =>
                       (m.SenderId == currentUser && m.ReceiverId == u.UserName) ||
                       (m.SenderId == u.UserName && m.ReceiverId == currentUser))
                   .OrderByDescending(m => m.Timestamp)
                   .FirstOrDefaultAsync();

               string lastMessageText = lastMsg?.Content ?? "";
               string timeAgo = lastMsg != null ? GetTimeAgo(lastMsg.Timestamp) : "";

               users.Add(new ChatUserVM
               {
                    Name = u.UserName ?? "",
                    LastMessage = lastMessageText,
                    TimeAgo = timeAgo,
                    AvatarUrl = "/images/avatar-placeholder.png",
                    IsOnline = false
               });
          }
     }

     private Task HandleUserSelected(string userName) => OnUserSelected.InvokeAsync(userName);

     private string GetTimeAgo(DateTime timestamp)
     {
          var span = DateTime.UtcNow - timestamp;
          if (span.TotalMinutes < 1)
               return "just now";
          if (span.TotalMinutes < 60)
               return $"{(int)span.TotalMinutes} min ago";
          if (span.TotalHours < 24)
               return $"{(int)span.TotalHours} hour{(span.TotalHours >= 2 ? "s" : "")} ago";
          return timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
     }

} 