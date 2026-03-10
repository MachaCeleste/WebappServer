using System.Text.Json.Serialization;

namespace WebappServer.NetworkMessages;

public class ClientMessage : Message
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonConstructor]
    private ClientMessage() { }

    public ClientMessage(int id)
    {
        this.ID = id;
    }
}
