using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChatClient.Services;

public class ChatService
{
    private HubConnection? _hubConnection;

    public event Action<string, string>? MessageReceived;

    public async Task StartAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5131/chathub")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            MessageReceived?.Invoke(user, message);
        });

        await _hubConnection.StartAsync();
    }

    public async Task SendMessage(string user, string message)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMessage", user, message);
        }
    }
}
