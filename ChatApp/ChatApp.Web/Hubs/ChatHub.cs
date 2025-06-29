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

     public async Task SendMessage(string sender, string receiver, string message, string? fileUrl, string? fileName, string? fileType)
     {
          var dbMessage = new Message
          {
               SenderId = sender,
               ReceiverId = receiver,
               Content = message,
               FileUrl = fileUrl,
               FileName = fileName,
               FileType = fileType,
               Timestamp = DateTime.UtcNow,
               IsRead = false
          };
          _db.Messages.Add(dbMessage);
          await _db.SaveChangesAsync();

          var chatMessage = new ChatMessage
          {
               Id = dbMessage.Id.ToString(),
               Sender = dbMessage.SenderId,
               Receiver = dbMessage.ReceiverId,
               Content = dbMessage.Content,
               Timestamp = dbMessage.Timestamp,
               IsRead = dbMessage.IsRead,
               FileUrl = dbMessage.FileUrl,
               FileName = dbMessage.FileName,
               FileType = dbMessage.FileType,
          };

          if (_userConnections.TryGetValue(receiver, out var connectionId))
          {
               await Clients.Client(connectionId).SendAsync("ReceiveMessage", chatMessage);
          }

          await Clients.Caller.SendAsync("MessageSent", chatMessage);
     }

     public async Task JoinChat(string username)
     {
          _userConnections[username] = Context.ConnectionId;
          await Clients.All.SendAsync("UserStatusChanged", username, true);
          await Clients.All.SendAsync("UserJoined", username);

          var recentDbMessages = _db.Messages
               .Where(m => m.SenderId == username || m.ReceiverId == username)
               .OrderByDescending(m => m.Timestamp)
               .Take(50)
               .Reverse()
               .ToList();

          var chatMessages = recentDbMessages.Select(m => new ChatMessage
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
          }).ToList();

          await Clients.Caller.SendAsync("LoadMessages", chatMessages);
     }

     public async Task LeaveChat(string username)
     {
          _userConnections.Remove(username);
          await Clients.All.SendAsync("UserStatusChanged", username, false);
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
                    if (_userConnections.TryGetValue(message.SenderId, out var senderConnectionId))
                    {
                         await Clients.Client(senderConnectionId).SendAsync("MessageRead", messageId);
                    }
               }
          }
     }

     public override async Task OnDisconnectedAsync(Exception? exception)
     {
          var userName = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
          if (!string.IsNullOrEmpty(userName))
          {
               _userConnections.Remove(userName);
               await Clients.All.SendAsync("UserStatusChanged", userName, false);
          }
          await base.OnDisconnectedAsync(exception);
     }
}