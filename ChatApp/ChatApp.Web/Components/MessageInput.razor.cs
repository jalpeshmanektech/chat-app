using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ChatApp.Web.Components;

public partial class MessageInput : ComponentBase, IDisposable
{
     [Inject] private ChatService ChatService { get; set; } = default!;
     [Inject] private IJSRuntime JS { get; set; } = default!;
     [Inject] private HttpClient Http { get; set; } = default!;
     [Parameter] public Func<string, string?, Task>? OnSendMessage { get; set; }
     [Parameter] public Func<Task>? OnTyping { get; set; }
     [Parameter] public Func<Task>? OnStopTyping { get; set; }
     private string MessageText { get; set; } = string.Empty;
     private ElementReference messageInput;
     private IBrowserFile? selectedFile;
     private Timer? typingTimer;

     protected override void OnInitialized()
     {
          Console.WriteLine("MessageInput component initialized");
          Console.WriteLine($"OnSendMessage is null: {OnSendMessage == null}");
          Console.WriteLine($"OnTyping is null: {OnTyping == null}");
          Console.WriteLine($"OnStopTyping is null: {OnStopTyping == null}");
     }

     private void OnFileSelected(InputFileChangeEventArgs e)
     {
          selectedFile = e.File;
     }

     private async Task<string?> UploadImageAsync()
     {
          if (selectedFile == null) return null;
          var content = new MultipartFormDataContent();
          content.Add(new StreamContent(selectedFile.OpenReadStream(10_000_000)), "file", selectedFile.Name);
          var response = await Http.PostAsync("https://localhost:7042/api/upload", content);
          if (response.IsSuccessStatusCode)
          {
               var json = await response.Content.ReadFromJsonAsync<JsonElement>();
               return json.GetProperty("url").GetString();
          }
          return null;
     }

     private async Task SendMessage()
     {
          Console.WriteLine("SendMessage called");
          Console.WriteLine($"MessageText: '{MessageText}'");

          string? imageUrl = null;
          if (selectedFile != null)
          {
               imageUrl = await UploadImageAsync();
               selectedFile = null;
          }

          if (!string.IsNullOrWhiteSpace(MessageText?.Trim()) || imageUrl != null)
          {
               var message = MessageText.Trim();
               MessageText = string.Empty;

               if (OnSendMessage != null)
               {
                    await OnSendMessage(message, imageUrl);
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
               Console.WriteLine("Message is empty or whitespace and no image");
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