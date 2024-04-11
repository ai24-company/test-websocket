using System.Text.Json.Serialization;

namespace TestWepApp;

public class DataDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("isMe")]
    public bool IsMe { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
}