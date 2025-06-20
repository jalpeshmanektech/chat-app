using Microsoft.AspNetCore.SignalR.Client;
using ChatApp.Web.Hubs;
using Microsoft.AspNetCore.Components;
using ChatApp.Web.ViewModels;

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
    public event Action<string>? MessageRead;

    public ChatService(ILogger<ChatService> logger, NavigationManager navigationManager)
    {
        _logger = logger;
        _navigationManager = navigationManager;
        _logger.LogInformation("ChatService created");
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting SignalR connection...");
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ChatMessage>("ReceiveMessage", (message) =>
        {
            _logger.LogInformation("Received message from SignalR: {Sender} -> {Receiver}: {Content}", 
                message.Sender, message.Receiver, message.Content);
            MessageReceived?.Invoke(message);
        });

        _hubConnection.On<ChatMessage>("MessageSent", (message) =>
        {
            _logger.LogInformation("Message sent confirmation: {Sender} -> {Receiver}: {Content}", 
                message.Sender, message.Receiver, message.Content);
            MessageSent?.Invoke(message);
        });

        _hubConnection.On<string>("UserJoined", (username) =>
        {
            _logger.LogInformation("User joined: {Username}", username);
            UserJoined?.Invoke(username);
        });

        _hubConnection.On<string>("UserLeft", (username) =>
        {
            _logger.LogInformation("User left: {Username}", username);
            UserLeft?.Invoke(username);
        });

        _hubConnection.On<string>("UserTyping", (username) =>
        {
            _logger.LogInformation("User typing: {Username}", username);
            UserTyping?.Invoke(username);
        });

        _hubConnection.On<string>("UserStoppedTyping", (username) =>
        {
            _logger.LogInformation("User stopped typing: {Username}", username);
            UserStoppedTyping?.Invoke(username);
        });

        _hubConnection.On<List<ChatMessage>>("LoadMessages", (messages) =>
        {
            _logger.LogInformation("Loaded {Count} messages from SignalR", messages.Count);
            MessagesLoaded?.Invoke(messages);
        });

        _hubConnection.On<string>("MessageRead", (messageId) =>
        {
            _logger.LogInformation("Message {MessageId} marked as read", messageId);
            MessageRead?.Invoke(messageId);
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
            _logger.LogInformation("SignalR connection started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection");
        }
    }

    public async Task JoinChatAsync(string username)
    {
        _logger.LogInformation("Joining chat as: {Username}", username);
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("JoinChat", username);
            _logger.LogInformation("JoinChat sent to hub for user: {Username}", username);
        }
        else
        {
            _logger.LogWarning("Cannot join chat - hub not connected. State: {State}", _hubConnection?.State);
        }
    }

    public async Task SendMessageAsync(string sender, string receiver, string message, string? fileUrl, string? fileName, string? fileType)
    {
        _logger.LogInformation("Sending message: {Sender} -> {Receiver}: {Content}, FileUrl: {FileUrl}, FileName: {FileName}, FileType: {FileType}", sender, receiver, message, fileUrl, fileName, fileType);
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("SendMessage", sender, receiver, message, fileUrl, fileName, fileType);
            _logger.LogInformation("SendMessage sent to hub");
        }
        else
        {
            _logger.LogWarning("Cannot send message - hub not connected. State: {State}", _hubConnection?.State);
        }
    }

    public async Task LeaveChatAsync(string username)
    {
        _logger.LogInformation("Leaving chat as: {Username}", username);
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("LeaveChat", username);
        }
    }

    public async Task TypingAsync(string sender, string receiver)
    {
        _logger.LogInformation("Typing notification: {Sender} -> {Receiver}", sender, receiver);
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("Typing", sender, receiver);
        }
    }

    public async Task StopTypingAsync(string sender, string receiver)
    {
        _logger.LogInformation("Stop typing notification: {Sender} -> {Receiver}", sender, receiver);
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
        _logger.LogInformation("Disposing ChatService");
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 