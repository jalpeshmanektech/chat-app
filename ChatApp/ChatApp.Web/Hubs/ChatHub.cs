using ChatApp.Web.Data;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Web.Hubs;

public class ChatHub : Hub
{

     private readonly ApplicationDbContext _db;
     private static readonly Dictionary<string, string> _userConnections = new();
     private static readonly List<ChatMessage> _messages = new();

     public ChatHub(ApplicationDbContext db)
     {
          _db = db;
     }

     public async Task SendMessage(string sender, string receiver, string message)
     {
          var chatMessage = new Message
          {
               SenderId = sender,
               ReceiverId = receiver,
               Content = message,
               Timestamp = DateTime.UtcNow,
               IsRead = false
          };
          _db.Messages.Add(chatMessage);
          await _db.SaveChangesAsync();

          // Send to specific user if they're online
          if (_userConnections.TryGetValue(receiver, out var connectionId))
          {
               await Clients.Client(connectionId).SendAsync("ReceiveMessage", chatMessage);
          }

          // Send back to sender for confirmation
          await Clients.Caller.SendAsync("MessageSent", chatMessage);
     }

     public async Task JoinChat(string username)
     {

          _userConnections[username] = Context.ConnectionId;
          await Clients.All.SendAsync("UserJoined", username);

          // Send recent messages to the user
          var recentMessages = _db.Messages.Where(m => m.SenderId == username || m.ReceiverId == username)
               .OrderByDescending(m => m.Timestamp).Take(50).ToList();
          await Clients.Caller.SendAsync("LoadMessages", recentMessages);
     }

     public async Task LeaveChat(string username)
     {
          _userConnections.Remove(username);
          await Clients.All.SendAsync("UserLeft", username);
     }

     public async Task Typing(string sender, string receiver)
     {
          if (_userConnections.TryGetValue(receiver, out var connectionId))
          {
               await Clients.Client(connectionId).SendAsync("UserTyping", sender);
          }
     }

     public async Task StopTyping(string sender, string receiver)
     {
          if (_userConnections.TryGetValue(receiver, out var connectionId))
          {
               await Clients.Client(connectionId).SendAsync("UserStoppedTyping", sender);
          }
     }

     public async Task MarkAsRead(string messageId, string username)
     {
          var message = _messages.FirstOrDefault(m => m.Id == messageId);
          if (message != null && message.Receiver == username)
          {
               message.IsRead = true;
               await Clients.Caller.SendAsync("MessageRead", messageId);
          }
     }

     public override async Task OnDisconnectedAsync(Exception? exception)
     {
          var username = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
          if (username != null)
          {
               _userConnections.Remove(username);
               await Clients.All.SendAsync("UserLeft", username);
          }

          await base.OnDisconnectedAsync(exception);
     }
}

public class ChatMessage
{
     public string Id { get; set; } = string.Empty;
     public string Sender { get; set; } = string.Empty;
     public string Receiver { get; set; } = string.Empty;
     public string Content { get; set; } = string.Empty;
     public DateTime Timestamp { get; set; }
     public bool IsRead { get; set; }
     public string? ImageUrl { get; set; }
     public string? FileUrl { get; set; }
     public string? FileName { get; set; }
}