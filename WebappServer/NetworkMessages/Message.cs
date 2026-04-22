using System.Text.Json.Serialization;

namespace WebappServer.NetworkMessages;

public class Message
{
    [JsonPropertyName("strValues")]
    public List<string> StrValues { get; set; } = new List<string>();

    [JsonPropertyName("intValues")]
    public List<int> IntValues { get; set; } = new List<int>();

    [JsonPropertyName("floatValues")]
    public List<float> FloatValues { get; set; } = new List<float>();

    [JsonConstructor]
    public Message() { }

    public void AddString(string value) => this.StrValues.Add(value);
    public void AddInt(int value) => this.IntValues.Add(value);
    public void AddFloat(float value) => this.FloatValues.Add(value);

    public string GetString()
    {
        string result = this.StrValues[0];
        this.StrValues.RemoveAt(0);
        return result;
    }

    public int GetInt()
    {
        int result = this.IntValues[0];
        this.IntValues.RemoveAt(0);
        return result;
    }

    public float GetFloat()
    {
        float result = this.FloatValues[0];
        this.FloatValues.RemoveAt(0);
        return result;
    }
}
