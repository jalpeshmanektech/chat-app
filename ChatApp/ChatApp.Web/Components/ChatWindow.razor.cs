using ChatApp.Web.Hubs;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ChatApp.Web.Components;

public partial class ChatWindow : ComponentBase
{
     [Inject] private ChatService ChatService { get; set; } = default!;
     [Inject] private IJSRuntime JS { get; set; } = default!;
     [Inject] private NavigationManager Navigation { get; set; } = default!;
     [Parameter] public string CurrentUser { get; set; } = "Me";
     [Parameter] public string CurrentChatUser { get; set; } = "Anna Nowak";
     [Parameter] public bool IsOnline { get; set; } = true;

     private List<ChatMessage> Messages { get; set; } = new();
     private bool IsTyping { get; set; } = false;
     private ElementReference messagesContainer;

     protected override async Task OnInitializedAsync()
     {
          Console.WriteLine("ChatWindow initialized");
          Console.WriteLine($"CurrentUser: {CurrentUser}");
          Console.WriteLine($"CurrentChatUser: {CurrentChatUser}");

          // Subscribe to chat events
          ChatService.MessageReceived += OnMessageReceived;
          ChatService.MessageSent += OnMessageSent;
          ChatService.MessagesLoaded += OnMessagesLoaded;
          ChatService.UserTyping += OnUserTyping;
          ChatService.UserStoppedTyping += OnUserStoppedTyping;

          // Start SignalR connection
          await ChatService.StartAsync();

          // Join chat
          await ChatService.JoinChatAsync(CurrentUser);
     }

     private async Task SendMessage(string message)
     {
          Console.WriteLine($"ChatWindow.SendMessage called with: '{message}'");
          if (!string.IsNullOrWhiteSpace(message))
          {
               await ChatService.SendMessageAsync(CurrentUser, CurrentChatUser, message);
          }
     }

     private async Task OnTyping()
     {
          Console.WriteLine("ChatWindow.OnTyping called");
          await ChatService.TypingAsync(CurrentUser, CurrentChatUser);
     }

     private async Task OnStopTyping()
     {
          Console.WriteLine("ChatWindow.OnStopTyping called");
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
               });
          }
     }

     private async void OnMessageSent(ChatMessage message)
     {
          // Message was sent successfully
          await InvokeAsync(() =>
          {
               StateHasChanged();
               ScrollToBottom();
          });
     }

     private async void OnMessagesLoaded(List<ChatMessage> messages)
     {
          await InvokeAsync(() =>
          {
               Messages = messages.Where(m =>
                   (m.Sender == CurrentUser && m.Receiver == CurrentChatUser) ||
                   (m.Sender == CurrentChatUser && m.Receiver == CurrentUser)
               ).ToList();
               StateHasChanged();
               ScrollToBottom();
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

     private async Task ScrollToBottom()
     {
          await Task.Delay(100); // Small delay to ensure DOM is updated
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
          // Unsubscribe from events
          ChatService.MessageReceived -= OnMessageReceived;
          ChatService.MessageSent -= OnMessageSent;
          ChatService.MessagesLoaded -= OnMessagesLoaded;
          ChatService.UserTyping -= OnUserTyping;
          ChatService.UserStoppedTyping -= OnUserStoppedTyping;

          // Leave chat
          await ChatService.LeaveChatAsync(CurrentUser);

          await ChatService.DisposeAsync();
     }
} 