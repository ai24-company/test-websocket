using System.Text.Json.Serialization;

namespace TestWepApp;

public class MessageDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}