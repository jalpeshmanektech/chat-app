using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ChatApp.Web.Components;

public partial class MessageInput : ComponentBase
{
     [Inject] private ChatService ChatService { get; set; } = default!;
     [Inject] private IJSRuntime JS { get; set; } = default!;
     [Parameter] public Func<string, Task>? OnSendMessage { get; set; }
     [Parameter] public Func<Task>? OnTyping { get; set; }
     [Parameter] public Func<Task>? OnStopTyping { get; set; }

     private string MessageText { get; set; } = string.Empty;
     private ElementReference messageInput;
     private Timer? typingTimer;

    
     protected override void OnInitialized()
     {
          Console.WriteLine("MessageInput component initialized");
          Console.WriteLine($"OnSendMessage is null: {OnSendMessage == null}");
          Console.WriteLine($"OnTyping is null: {OnTyping == null}");
          Console.WriteLine($"OnStopTyping is null: {OnStopTyping == null}");
     }

     private async Task SendMessage()
     {
          Console.WriteLine("SendMessage called");
          Console.WriteLine($"MessageText: '{MessageText}'");

          if (!string.IsNullOrWhiteSpace(MessageText?.Trim()))
          {
               var message = MessageText.Trim();
               Console.WriteLine($"Sending message: '{message}'");
               MessageText = string.Empty;

               if (OnSendMessage != null)
               {
                    await OnSendMessage(message);
               }
               else
               {
                    Console.WriteLine("OnSendMessage is null!");
               }

               await StopTypingIndicator();
               await AutoResizeTextarea();
          }
          else
          {
               Console.WriteLine("Message is empty or whitespace");
          }
     }

     private async Task OnKeyDown(KeyboardEventArgs e)
     {
          Console.WriteLine($"OnKeyDown: Key={e.Key}, Shift={e.ShiftKey}, Code={e.Code}");

          if (e.Key == "Enter" && !e.ShiftKey)
          {
               Console.WriteLine("Enter pressed without Shift - calling SendMessage");
               await SendMessage();
          }
     }

     private async Task OnKeyUp(KeyboardEventArgs e)
     {
          Console.WriteLine($"OnKeyUp: Key={e.Key}");
          await StartTypingIndicator();
          await AutoResizeTextarea();
     }

     private async Task StartTypingIndicator()
     {
          if (OnTyping != null)
          {
               await OnTyping();
          }

          // Reset typing timer
          typingTimer?.Dispose();
          typingTimer = new Timer(async _ =>
          {
               await InvokeAsync(async () =>
               {
                    await StopTypingIndicator();
                    StateHasChanged();
               });
          }, null, 2000, Timeout.Infinite);
     }

     private async Task StopTypingIndicator()
     {
          typingTimer?.Dispose();
          typingTimer = null;

          if (OnStopTyping != null)
          {
               await OnStopTyping();
          }
     }

     private async Task AutoResizeTextarea()
     {
          try
          {
               await JS.InvokeVoidAsync("autoResizeTextarea", messageInput);
          }
          catch
          {
               // Ignore JS errors
          }
     }

     public void Dispose()
     {
          typingTimer?.Dispose();
     }
} 