using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebappServer.NetworkMessages;
using System.Security.Claims;

namespace WebappServer;

public class AppServer
{
    private static readonly Lazy<AppServer> _singleton = new(() => new AppServer());
    public static AppServer Singleton => _singleton.Value;

    private readonly List<ClientHandler> _clients = new();

    public event Action<string, ServerMessage>? OnMessageReceived;
    public event Action<string>? OnClientConnected;
    public event Action<string>? OnClientDisconnect;

    private string _secretKey = "default";

    public void SetSecret(string secretKey) => _secretKey = secretKey;

    public (string AccessToken, string RefreshToken) GrantTokens(string? existingUserId = null)
    {
        string userId = existingUserId ?? Guid.NewGuid().ToString();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var accessDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("sub", userId) }),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = creds
        };

        var refreshDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("sub", userId) }),
            Expires = DateTime.UtcNow.AddYears(4),
            SigningCredentials = creds
        };

        var handler = new JwtSecurityTokenHandler();
        return (
            handler.WriteToken(handler.CreateToken(accessDescriptor)),
            handler.WriteToken(handler.CreateToken(refreshDescriptor))
            );
    }

    public async Task HandleNewConnection(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest) return;

        string? token = context.Request.Query["token"];
        string? userId = ValidateToken(token);

        if (userId == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        ClientHandler client = new ClientHandler(userId, webSocket);
        _clients.Add(client);
        OnClientConnected?.Invoke(userId);

        await client.ListenConnection();

        _clients.Remove(client);
        OnClientDisconnect?.Invoke(userId);
    }

    public async Task MessageAllClients(ClientMessage message)
    {
        foreach (var client in _clients.Where(x => x.Socket.State == WebSocketState.Open))
        {
            await client.SendMessage(message);
        }
    }

    public ClientHandler? GetClient(string userId)
    {
        return _clients.FirstOrDefault(x => x.UserId == userId);
    }

    public string? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claims = handler.ValidateToken(token, validations, out _);
            return claims.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? claims.FindFirst("sub")?.Value;
        }
        catch { return null; }
    }

    internal void MessageReceived(string userId, ServerMessage message)
    {
        OnMessageReceived?.Invoke(userId, message);
    }
}
