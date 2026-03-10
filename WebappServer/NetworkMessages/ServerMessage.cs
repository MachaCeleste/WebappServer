using System.Text.Json.Serialization;

namespace WebappServer.NetworkMessages;

public class ServerMessage : Message
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonConstructor]
    private ServerMessage() { }

    public ServerMessage(int id)
    {
        this.ID = id;
    }
}
