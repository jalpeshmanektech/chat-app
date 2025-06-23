using ChatApp.Web.Hubs;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using ChatApp.Web.ViewModels;

namespace ChatApp.Web.Components;

public partial class ChatWindow : ComponentBase
{
     [Inject] private ChatService ChatService { get; set; } = default!;
     [Inject] private IJSRuntime JS { get; set; } = default!;
     [Parameter] public string CurrentUser { get; set; } = null!;
     [Parameter] public string CurrentChatUser { get; set; } = null!;
     [Parameter] public bool IsOnline { get; set; } = true;
     [Parameter] public EventCallback<ChatMessage> OnMessageActivity { get; set; }
     private List<ChatMessage> Messages { get; set; } = new();
     private bool IsTyping { get; set; } = false;
     private ElementReference messagesContainer;
     private string? _lastChatUser;

     protected override async Task OnParametersSetAsync()
     {
          if (_lastChatUser != CurrentChatUser)
          {
               _lastChatUser = CurrentChatUser;
               Messages.Clear();
               await ChatService.JoinChatAsync(CurrentUser);
               StateHasChanged();
          }
     }
     protected override async Task OnInitializedAsync()
     {
          ChatService.MessageReceived += OnMessageReceived;
          ChatService.MessageSent += OnMessageSent;
          ChatService.MessagesLoaded += OnMessagesLoaded;
          ChatService.UserTyping += OnUserTyping;
          ChatService.UserStoppedTyping += OnUserStoppedTyping;
          ChatService.MessageRead += OnMessageRead;

          await ChatService.StartAsync();

          await ChatService.JoinChatAsync(CurrentUser);
     }

     private async Task SendMessage(string message, string? fileUrl, string? fileName, string? fileType)
     {
          if (!string.IsNullOrWhiteSpace(message) || !string.IsNullOrEmpty(fileUrl))
          {
               await ChatService.SendMessageAsync(CurrentUser, CurrentChatUser, message, fileUrl, fileName, fileType);
          }
     }

     private async Task OnTyping()
     {
          await ChatService.TypingAsync(CurrentUser, CurrentChatUser);
     }

     private async Task OnStopTyping()
     {
          await ChatService.StopTypingAsync(CurrentUser, CurrentChatUser);
     }

     private async void OnMessageReceived(ChatMessage message)
     {
          if ((message.Sender == CurrentUser && message.Receiver == CurrentChatUser) ||
              (message.Sender == CurrentChatUser && message.Receiver == CurrentUser))
          {
               await InvokeAsync(() =>
               {
                    Messages.Add(message);
                    StateHasChanged();
                    ScrollToBottom();

                    if (message.Receiver == CurrentUser)
                    {
                        ChatService.MarkAsReadAsync(message.Id, CurrentUser);
                    }
               });
               if (OnMessageActivity.HasDelegate)
               {
                    await InvokeAsync(() => OnMessageActivity.InvokeAsync(message));
               }
          }
     }

     private async void OnMessageSent(ChatMessage message)
     {
          await InvokeAsync(() =>
          {
               if (!Messages.Any(m => m.Id == message.Id))
               {
                    Messages.Add(message);
               }
               StateHasChanged();
               ScrollToBottom();
          });
          if (OnMessageActivity.HasDelegate)
          {
               await InvokeAsync(() => OnMessageActivity.InvokeAsync(message)); 
          }
     }

     private async void OnMessagesLoaded(List<ChatMessage> messages)
     {
          await InvokeAsync(() =>
          {
               var filteredMessages = messages.Where(m =>
                   (m.Sender == CurrentUser && m.Receiver == CurrentChatUser) ||
                   (m.Sender == CurrentChatUser && m.Receiver == CurrentUser)
               ).ToList();
               
               Messages = filteredMessages;
               StateHasChanged();
               ScrollToBottom();
               MarkVisibleMessagesAsReadAsync();
          });
     }

     private async void OnUserTyping(string username)
     {
          if (username == CurrentChatUser)
          {
               await InvokeAsync(() =>
               {
                    IsTyping = true;
                    StateHasChanged();
               });
          }
     }

     private async void OnUserStoppedTyping(string username)
     {
          if (username == CurrentChatUser)
          {
               await InvokeAsync(() =>
               {
                    IsTyping = false;
                    StateHasChanged();
               });
          }
     }

     private async void OnMessageRead(string messageId)
     {
         await InvokeAsync(() =>
         {
             var message = Messages.FirstOrDefault(m => m.Id == messageId);
             if (message != null)
             {
                 message.IsRead = true;
                 StateHasChanged();
             }
         });
     }

     private async Task MarkVisibleMessagesAsReadAsync()
     {
         var unreadMessages = Messages
             .Where(m => m.Receiver == CurrentUser && !m.IsRead)
             .ToList();
         
         foreach (var message in unreadMessages)
         {
             await ChatService.MarkAsReadAsync(message.Id, CurrentUser);
         }
     }

     private async Task ScrollToBottom()
     {
          await Task.Delay(100);
          try
          {
               await JS.InvokeVoidAsync("scrollToBottom", messagesContainer);
          }
          catch
          {
               // Ignore JS errors
          }
     }

     public async ValueTask DisposeAsync()
     {
          ChatService.MessageReceived -= OnMessageReceived;
          ChatService.MessageSent -= OnMessageSent;
          ChatService.MessagesLoaded -= OnMessagesLoaded;
          ChatService.UserTyping -= OnUserTyping;
          ChatService.UserStoppedTyping -= OnUserStoppedTyping;
          ChatService.MessageRead -= OnMessageRead;

          await ChatService.LeaveChatAsync(CurrentUser);

          await ChatService.DisposeAsync();
     }
} 