using System.Text.Json.Serialization;

namespace TestWepApp;

public class PythonMessageDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type_message")]
    public string TypeMessage { get; set; }
}