using ChatApp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ChatApp.Web.Components;

public partial class MessageInput : ComponentBase, IDisposable
{
     [Inject] private ChatService ChatService { get; set; } = default!;
     [Inject] private IJSRuntime JS { get; set; } = default!;
     [Inject] private HttpClient Http { get; set; } = default!;
     [Parameter] public Func<string, string?, string?, string?, Task>? OnSendMessage { get; set; }
     [Parameter] public Func<Task>? OnTyping { get; set; }
     [Parameter] public Func<Task>? OnStopTyping { get; set; }
     private string MessageText { get; set; } = string.Empty;
     private ElementReference messageInput;
     private DotNetObjectReference<MessageInput>? dotNetHelper;
     private IBrowserFile? selectedFile;
     private Timer? typingTimer;
     private string? fileValidationError;

     protected override void OnInitialized()
     {
          dotNetHelper = DotNetObjectReference.Create(this);
          Console.WriteLine("MessageInput component initialized");
          Console.WriteLine($"OnSendMessage is null: {OnSendMessage == null}");
          Console.WriteLine($"OnTyping is null: {OnTyping == null}");
          Console.WriteLine($"OnStopTyping is null: {OnStopTyping == null}");
     }

     private async Task ShowEmojiPicker()
     {
          await JS.InvokeVoidAsync("emojiInterop.showPicker", dotNetHelper);
     }

     [JSInvokable]
     public Task ReceiveEmoji(string emoji)
     {
          MessageText += emoji;
          StateHasChanged();
          return Task.CompletedTask;
     }

     private void OnFileSelected(InputFileChangeEventArgs e)
     {
          selectedFile = e.File;
          fileValidationError = null;

          if (!FileTypeHelper.IsFileTypeAllowed(selectedFile.Name))
          {
               fileValidationError = $"File type not allowed. Allowed types: {FileTypeHelper.GetAllowedExtensionsString()}";
               selectedFile = null;
               return;
          }

          if (!FileTypeHelper.IsFileSizeAllowed(selectedFile.Size))
          {
               fileValidationError = "File size too large. Maximum size is 10MB.";
               selectedFile = null;
               return;
          }

          Console.WriteLine($"File selected: {selectedFile.Name}, Size: {selectedFile.Size}, Type: {FileTypeHelper.GetFileTypeFromExtension(selectedFile.Name)}");
     }

     private void ClearSelectedFile()
     {
          selectedFile = null;
          fileValidationError = null;
     }

     private void ClearFileValidationError()
     {
          fileValidationError = null;
     }

     private async Task<string?> UploadFileAsync()
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
          if (string.IsNullOrWhiteSpace(MessageText?.Trim()) && selectedFile == null)
               return;

          var messageToSend = MessageText.Trim();
          MessageText = string.Empty;
          StateHasChanged();
          string? fileUrl = null;
          string? fileName = null;
          string? fileType = null;

          if (selectedFile != null)
          {
               fileUrl = await UploadFileAsync();
               fileName = selectedFile.Name;
               fileType = FileTypeHelper.GetFileTypeFromExtension(selectedFile.Name);
               selectedFile = null;
          }

          if (OnSendMessage != null)
          {
               await OnSendMessage(messageToSend, fileUrl, fileName, fileType);
          }
          await StopTypingIndicator();
          await AutoResizeTextarea();
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
          }
     }

     public void Dispose()
     {
          typingTimer?.Dispose();
          dotNetHelper?.Dispose();
     }
}