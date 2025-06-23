using ChatApp.Web.Data;
using ChatApp.Web.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Web.Components;

public partial class ChatList : ComponentBase
{
     [Parameter] public List<ChatUserVM> Users { get; set; } = new();
     [Parameter] public List<ChatMessage> Messages { get; set; } = new();
     [Parameter] public EventCallback<string> OnUserSelected { get; set; }
     private string searchTerm = string.Empty;

     private IEnumerable<ChatUserVM> FilteredUsers =>
         string.IsNullOrWhiteSpace(searchTerm)
             ? Users
             : Users.Where(u =>
                 u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 Messages.Any(m =>
                     (m.Sender == u.Name || m.Receiver == u.Name) &&
                     m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
     private Task HandleUserSelected(string userName) => OnUserSelected.InvokeAsync(userName);

} 