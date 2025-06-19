using ChatApp.Web.Data;
using ChatApp.Web.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Web.Hubs;

public class ChatHub : Hub
{

     private readonly ApplicationDbContext _db;
     private static readonly Dictionary<string, string> _userConnections = new();

     public ChatHub(ApplicationDbContext db)
     {
          _db = db;
     }

     public async Task SendMessage(string sender, string receiver, string message, string? imageUrl)
     {
          var dbMessage = new Message
          {
               SenderId = sender,
               ReceiverId = receiver,
               Content = message,
               ImageUrl = imageUrl,
               Timestamp = DateTime.UtcNow,
               IsRead = false
          };
          _db.Messages.Add(dbMessage);
          await _db.SaveChangesAsync();

          // Convert to ChatMessage for SignalR
          var chatMessage = new ChatMessage
          {
               Id = dbMessage.Id.ToString(),
               Sender = dbMessage.SenderId,
               Receiver = dbMessage.ReceiverId,
               Content = dbMessage.Content,
               Timestamp = dbMessage.Timestamp,
               IsRead = dbMessage.IsRead,
               ImageUrl = dbMessage.ImageUrl,
               FileUrl = dbMessage.FileUrl,
               FileName = dbMessage.FileName
          };

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
          var recentDbMessages = _db.Messages
               .Where(m => m.SenderId == username || m.ReceiverId == username)
               .OrderByDescending(m => m.Timestamp)
               .Take(50)
               .Reverse()
               .ToList();

          // Convert to ChatMessage for SignalR
          var chatMessages = recentDbMessages.Select(m => new ChatMessage
          {
               Id = m.Id.ToString(),
               Sender = m.SenderId,
               Receiver = m.ReceiverId,
               Content = m.Content,
               Timestamp = m.Timestamp,
               IsRead = m.IsRead,
               ImageUrl = m.ImageUrl,
               FileUrl = m.FileUrl,
               FileName = m.FileName
          }).ToList();

          await Clients.Caller.SendAsync("LoadMessages", chatMessages);
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
          if (int.TryParse(messageId, out var id))
          {
               var message = _db.Messages.FirstOrDefault(m => m.Id == id);
               if (message != null && message.ReceiverId == username)
               {
                    message.IsRead = true;
                    await _db.SaveChangesAsync();
                    await Clients.Caller.SendAsync("MessageRead", messageId);
               }
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