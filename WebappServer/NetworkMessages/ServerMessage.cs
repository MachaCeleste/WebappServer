using System.Text.Json.Serialization;

namespace WebappServer.NetworkMessages;

public class ServerMessage : Message
{
    [JsonPropertyName("id")]
    public int ID { get; private set; }

    [JsonPropertyName("sequence")]
    public int Sequence { get; private set; }

    [JsonConstructor]
    private ServerMessage() { }

    public ServerMessage(int id, int seq)
    {
        this.ID = id;
        this.Sequence = seq;
    }
}
