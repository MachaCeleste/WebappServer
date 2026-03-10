using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebappServer.NetworkMessages;

namespace WebappServer;

public class ClientHandler
{
    public string UserId { get; set; }
    public int Sequence { get; set; }
    internal WebSocket Socket { get; set; }

    internal ClientHandler(string userId, WebSocket socket)
    {
        UserId = userId;
        Socket = socket;
        Sequence = 0;
    }

    internal async Task ListenConnection()
    {
        var buffer = new byte[1024 * 4];
        while (Socket.State == WebSocketState.Open)
        {
            var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<ServerMessage>(json);
                if (message == null || message.Sequence <= Sequence) continue;
                Sequence++;
                AppServer.Singleton.MessageReceived(this.UserId, message);
            }
        }
    }

    public async Task SendMessage(ClientMessage message)
    {
        string json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
