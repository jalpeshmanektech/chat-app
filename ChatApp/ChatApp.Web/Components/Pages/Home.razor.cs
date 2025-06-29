﻿using ChatApp.Web.Data;
using ChatApp.Web.Services;
using ChatApp.Web.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Web.Components.Pages
{
     public partial class Home
     {
          [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
          [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
          [Inject] private ApplicationDbContext DbContext { get; set; } = default!;
          [Inject] private ChatService ChatService { get; set; } = default!;
          [Inject] private NavigationManager Navigation { get; set; } = default!;
          private string CurrentUser { get; set; } = null!;
          private string CurrentChatUser { get; set; } = null!;
          private List<ChatUserVM> ChatUsers { get; set; } = new();
          private List<ChatMessage> CurrentChatMessages { get; set; } = new();
          private List<ChatMessage> AllMessages { get; set; } = new();
          private ChatUserVM? SelectedChatUser => ChatUsers.FirstOrDefault(u => u.Name == CurrentChatUser);
          private bool IsChatWindow = false;
          private bool IsSidebarActive = false;
          protected override async Task OnInitializedAsync()
          {
               var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
               var user = authState.User;
               if (user.Identity?.IsAuthenticated != true)
               {
                    Navigation.NavigateTo("/account/login", true);
                    return;
               }
               CurrentUser = user.Identity.Name ?? "Unknown";
               var dbUsers = UserManager.Users.ToList();
               ChatUsers = new List<ChatUserVM>();

               foreach (var u in dbUsers)
               {
                    if (u.UserName == CurrentUser)
                         continue;

                    var lastMsg = DbContext.Messages
                        .Where(m =>
                            (m.SenderId == CurrentUser && m.ReceiverId == u.UserName) ||
                            (m.SenderId == u.UserName && m.ReceiverId == CurrentUser))
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefault();

                    string lastMessageText = lastMsg?.Content ?? "";
                    string timeAgo = lastMsg != null ? GetTimeAgo(lastMsg.Timestamp) : "";

                    ChatUsers.Add(new ChatUserVM
                    {
                         Name = u.UserName ?? "",
                         LastMessage = lastMessageText,
                         TimeAgo = timeAgo,
                         AvatarUrl = "/images/no-profile.png",
                         IsOnline = false
                    });
               }

               if (ChatUsers.Any())
               {
                    CurrentChatUser = ChatUsers.First().Name;
                    await LoadCurrentChatMessages();
               }

               ChatService.UserStatusChanged += (userName, isOnline) =>
               {
                    var user = ChatUsers.FirstOrDefault(u => u.Name == userName);
                    if (user != null)
                    {
                         user.IsOnline = isOnline;
                         InvokeAsync(StateHasChanged);
                    }
               };
          }

          private void OnBackFromChat()
          {
               IsChatWindow = false;
               StateHasChanged();
          }

          private void OnSidebarToggle(bool flag)
          {
               IsSidebarActive = flag;
               StateHasChanged();
          }

          private async void OnMessageActivity(ChatMessage message)
          {
               UpdateChatListWithMessage(message);
               await LoadCurrentChatMessages();
               await InvokeAsync(StateHasChanged);
          }

          private void UpdateChatListWithMessage(ChatMessage message)
          {
               var chatUser = ChatUsers.FirstOrDefault(u => u.Name == (message.Sender == CurrentUser ? message.Receiver : message.Sender));
               if (chatUser != null)
               {
                    chatUser.LastMessage = message.Content;
                    chatUser.TimeAgo = GetTimeAgo(message.Timestamp);
               }
               ChatUsers = ChatUsers
                    .OrderByDescending(u =>
                    {
                         var lastMsg = DbContext.Messages
                              .Where(m => (m.SenderId == CurrentUser && m.ReceiverId == u.Name) ||
                                          (m.SenderId == u.Name && m.ReceiverId == CurrentUser))
                              .OrderByDescending(m => m.Timestamp)
                              .FirstOrDefault();
                         return lastMsg?.Timestamp ?? DateTime.MinValue;
                    })
                    .ToList();
          }

          private void SetCurrentChatUser(string userName)
          {
               CurrentChatUser = userName;
               IsChatWindow = true;
               LoadCurrentChatMessages().GetAwaiter().GetResult();
          }

          private async Task LoadCurrentChatMessages()
          {
               if (!string.IsNullOrEmpty(CurrentUser) && !string.IsNullOrEmpty(CurrentChatUser))
               {
                    AllMessages = DbContext.Messages
                        .Select(m => new ChatMessage
                        {
                             Id = m.Id.ToString(),
                             Sender = m.SenderId,
                             Receiver = m.ReceiverId,
                             Content = m.Content,
                             Timestamp = m.Timestamp,
                             IsRead = m.IsRead,
                             FileUrl = m.FileUrl,
                             FileName = m.FileName,
                             FileType = m.FileType
                        })
                        .ToList();
                    CurrentChatMessages = AllMessages
                        .Where(m => (m.Sender == CurrentUser && m.Receiver == CurrentChatUser) ||
                                    (m.Sender == CurrentChatUser && m.Receiver == CurrentUser))
                        .OrderBy(m => m.Timestamp).ToList();
               }
               else
               {
                    CurrentChatMessages = new List<ChatMessage>();
               }
          }

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
}
