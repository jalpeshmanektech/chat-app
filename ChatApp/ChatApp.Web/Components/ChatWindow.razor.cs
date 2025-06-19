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

     private List<ChatMessage> Messages { get; set; } = new();
     private bool IsTyping { get; set; } = false;
     private ElementReference messagesContainer;

     protected override async Task OnInitializedAsync()
     {
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
          Console.WriteLine($"OnMessageReceived called: {message.Sender} -> {message.Receiver}: {message.Content}");
          Console.WriteLine($"Message ID: {message.Id}");
          Console.WriteLine($"Current Messages count: {Messages.Count}");
          
          if ((message.Sender == CurrentUser && message.Receiver == CurrentChatUser) ||
              (message.Sender == CurrentChatUser && message.Receiver == CurrentUser))
          {
               await InvokeAsync(() =>
               {
                    Messages.Add(message);
                    Console.WriteLine($"Message added to list. New count: {Messages.Count}");
                    StateHasChanged();
                    ScrollToBottom();
               });
          }
          else
          {
               Console.WriteLine("Message filtered out - not for current chat");
          }
     }

     private async void OnMessageSent(ChatMessage message)
     {
          Console.WriteLine($"OnMessageSent called: {message.Sender} -> {message.Receiver}: {message.Content}");
          Console.WriteLine($"Message ID: {message.Id}");
          Console.WriteLine($"Current Messages count: {Messages.Count}");
          
          // Message was sent successfully - add it to the local messages list
          await InvokeAsync(() =>
          {
               // Only add if it's not already in the list (to avoid duplicates)
               if (!Messages.Any(m => m.Id == message.Id))
               {
                    Messages.Add(message);
                    Console.WriteLine($"Message added to list. New count: {Messages.Count}");
               }
               else
               {
                    Console.WriteLine("Message already exists in list, skipping");
               }
               StateHasChanged();
               ScrollToBottom();
          });
     }

     private async void OnMessagesLoaded(List<ChatMessage> messages)
     {
          Console.WriteLine($"OnMessagesLoaded called with {messages.Count} messages");
          Console.WriteLine($"CurrentUser: {CurrentUser}, CurrentChatUser: {CurrentChatUser}");
          
          await InvokeAsync(() =>
          {
               var filteredMessages = messages.Where(m =>
                   (m.Sender == CurrentUser && m.Receiver == CurrentChatUser) ||
                   (m.Sender == CurrentChatUser && m.Receiver == CurrentUser)
               ).ToList();
               
               Console.WriteLine($"Filtered to {filteredMessages.Count} messages for current chat");
               
               Messages = filteredMessages;
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