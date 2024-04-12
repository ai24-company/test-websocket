using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TestWepApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ItemService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAnyOrigin");

app.MapGet("/create-stream", async (HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
    
    var dataStart = new DataDto { Message = "", Id = "first", IsMe = false, Type = "start" };
    var dataStream = new DataDto { Message = "Добро пожаловать в чат", Id = "first", IsMe = false, Type = "stream" };
    var dataEnd = new DataDto { Message = "", Id = "first", IsMe = false, Type = "end" };
        
    await ctx.Response.WriteAsync("data:");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, dataStart);
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.WriteAsync("data:");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, dataStream);
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.WriteAsync("data:");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, dataEnd);
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.Body.FlushAsync();
});

app.MapPost("/send-text", async (MessageDto dto ,HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
    ctx.Response.Headers.Append("Connection", "keep-alive");
    ctx.Response.Headers.Append("Cache-Control", "no-cache");

    var idOld = Guid.NewGuid().ToString();
    await ctx.Response.WriteAsync("data: ");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = idOld, IsMe = true, Type = "start"});
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.WriteAsync("data: ");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = dto.Message, Id = idOld, IsMe = true, Type = "stream"});
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.WriteAsync("data: ");
    await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = idOld, IsMe = true, Type = "end"});
    await ctx.Response.WriteAsync("\n\n");
    await ctx.Response.Body.FlushAsync();
    
    var id = Guid.NewGuid().ToString();
    while (!ct.IsCancellationRequested)
    {
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = id, IsMe = false, Type = "start"});
        await ctx.Response.WriteAsync("\n\n");
        await foreach (var message in GetMessagesFromPython(dto.Message))
        {
            await ctx.Response.WriteAsync("data: ");
            await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = message, Id = id, IsMe = false, Type = "stream"});
            await ctx.Response.WriteAsync("\n\n");
            await ctx.Response.Body.FlushAsync();
        }
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = id, IsMe = false, Type = "end"});
        await ctx.Response.WriteAsync("\n\n");
    }
});

app.Run();

async IAsyncEnumerable<string> GetMessagesFromPython(string message)
{
    var uri = new Uri("ws://localhost:8000/api/chat/");
    using var ws = new ClientWebSocket();
    await ws.ConnectAsync(uri, default);
    var options = new JsonSerializerOptions
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    var messageToSend = new { message };
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
            yield return receivedString;
        }
    }
}