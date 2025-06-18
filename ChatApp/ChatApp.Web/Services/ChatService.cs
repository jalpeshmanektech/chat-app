using Microsoft.AspNetCore.SignalR.Client;
using ChatApp.Web.Hubs;

namespace ChatApp.Web.Services;

public class ChatService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly ILogger<ChatService> _logger;
    private readonly NavigationManager _navigationManager;

    public event Action<ChatMessage>? MessageReceived;
    public event Action<ChatMessage>? MessageSent;
    public event Action<string>? UserJoined;
    public event Action<string>? UserLeft;
    public event Action<string>? UserTyping;
    public event Action<string>? UserStoppedTyping;
    public event Action<List<ChatMessage>>? MessagesLoaded;

    public ChatService(ILogger<ChatService> logger, NavigationManager navigationManager)
    {
        _logger = logger;
        _navigationManager = navigationManager;
    }

    public async Task StartAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ChatMessage>("ReceiveMessage", (message) =>
        {
            MessageReceived?.Invoke(message);
        });

        _hubConnection.On<ChatMessage>("MessageSent", (message) =>
        {
            MessageSent?.Invoke(message);
        });

        _hubConnection.On<string>("UserJoined", (username) =>
        {
            UserJoined?.Invoke(username);
        });

        _hubConnection.On<string>("UserLeft", (username) =>
        {
            UserLeft?.Invoke(username);
        });

        _hubConnection.On<string>("UserTyping", (username) =>
        {
            UserTyping?.Invoke(username);
        });

        _hubConnection.On<string>("UserStoppedTyping", (username) =>
        {
            UserStoppedTyping?.Invoke(username);
        });

        _hubConnection.On<List<ChatMessage>>("LoadMessages", (messages) =>
        {
            MessagesLoaded?.Invoke(messages);
        });

        _hubConnection.Closed += async (error) =>
        {
            _logger.LogError(error, "SignalR connection closed");
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await StartAsync();
        };

        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("SignalR connection started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection");
        }
    }

    public async Task JoinChatAsync(string username)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("JoinChat", username);
        }
    }

    public async Task SendMessageAsync(string sender, string receiver, string message)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("SendMessage", sender, receiver, message);
        }
    }

    public async Task LeaveChatAsync(string username)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("LeaveChat", username);
        }
    }

    public async Task TypingAsync(string sender, string receiver)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("Typing", sender, receiver);
        }
    }

    public async Task StopTypingAsync(string sender, string receiver)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("StopTyping", sender, receiver);
        }
    }

    public async Task MarkAsReadAsync(string messageId, string username)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("MarkAsRead", messageId, username);
        }
    }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 