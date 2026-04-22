using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using WebappServer.NetworkMessages;

namespace WebappServer;

public class AppServer
{
    public static AppServer Singleton { get; private set; }

    private TokenHelper _tokenHelper { get; }

    private readonly List<ClientHandler> _clients = new();

    public event Action<string, ServerMessage>? OnMessageReceived;
    public event Action<string>? OnClientConnect;
    public event Action<string>? OnClientDisconnect;

    public AppServer(string secretKey)
    {
        if (Singleton != null)
            throw new InvalidOperationException("AppServer is already instantiated!");

        _tokenHelper = new TokenHelper(secretKey);
        Singleton = this;
    }

    public async Task HandleNewConnection(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest) return;

        string? token = context.Request.Query["token"];
        string? userId = _tokenHelper.ValidateToken(token);

        if (userId == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        ClientHandler client = new ClientHandler(userId, webSocket);
        _clients.Add(client);
        OnClientConnect?.Invoke(userId);

        await client.ListenConnection();

        _clients.Remove(client);
        OnClientDisconnect?.Invoke(userId);
    }

    public async Task MessageAllClients(ClientMessage message)
    {
        foreach (var client in _clients.Where(x => x.Socket.State == WebSocketState.Open))
            await client.SendMessage(message);
    }

    public ClientHandler? GetClient(string userId) => _clients.FirstOrDefault(x => x.UserId == userId);

    internal void MessageReceived(string userId, ServerMessage message) => OnMessageReceived?.Invoke(userId, message);
}
