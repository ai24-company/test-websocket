using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

var uri = new Uri("ws://localhost:8000/api/chat/");
using var ws = new ClientWebSocket();
await ws.ConnectAsync(uri, default);

var options = new JsonSerializerOptions
{
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Optional: Allow special characters
};
var messageToSend = new { message = "Hello from .NET!" };
var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(messageToSend, options);

await ws.SendAsync(jsonUtf8Bytes, WebSocketMessageType.Text, true, default);
while (ws.State == WebSocketState.Open)
{
    var buffer = new ArraySegment<byte>(new byte[4096]);
    var result = await ws.ReceiveAsync(buffer, default);

    if (result.MessageType == WebSocketMessageType.Close)
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default);
        break;
    }
    else if (result.MessageType == WebSocketMessageType.Text)
    {
        var receivedString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        Console.WriteLine($"Received message: {receivedString}");
    }
}

var connections = new Dictionary<string, WebSocket>();

app.Run();

app.MapGet("/api/chat", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        string connectionId = Guid.NewGuid().ToString();
        connections.Add(connectionId, webSocket);

        var messageToSend = new { message = "Hello from .NET!" };
        var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(messageToSend, options);

        await webSocket.SendAsync(jsonUtf8Bytes, WebSocketMessageType.Text, true, default);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});