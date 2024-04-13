using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TestWepApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAnyOrigin");

var dict = new Dictionary<string, ClientWebSocket>();
app.MapPost("/send-text", async ([FromBody]string incomeMessage, [FromBody]string typeChat, [FromBody]string token, HttpContext ctx, CancellationToken ct) =>
{
    if (typeChat == "init")
    {
        ctx.Response.Headers.Append("Content-Type", "text/event-stream");
        ctx.Response.Headers.Append("Cache-Control", "no-cache");

        var incomeMessageId = Guid.NewGuid().ToString();
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = incomeMessageId, Sender = "bot", Type = "start"});
        await ctx.Response.WriteAsync("\n\n");

        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "Welcome", Id = incomeMessageId, Sender = "bot", Type = "stream"});
        await ctx.Response.WriteAsync("\n\n");
        
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = incomeMessageId, Sender = "bot", Type = "end"});
        await ctx.Response.WriteAsync("\n\n");
        await ctx.Response.Body.FlushAsync();
    }
    else
    {
        ctx.Response.Headers.Append("Content-Type", "text/event-stream");
        ctx.Response.Headers.Append("Connection", "keep-alive");
        ctx.Response.Headers.Append("Cache-Control", "no-cache");
        var incomeMessageId = Guid.NewGuid().ToString();
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = incomeMessageId, Sender = "user", Type = "start"});
        await ctx.Response.WriteAsync("\n\n");

        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = incomeMessage, Id = incomeMessageId, Sender = "user", Type = "stream"});
        await ctx.Response.WriteAsync("\n\n");
        
        await ctx.Response.WriteAsync("data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = incomeMessageId, Sender = "user", Type = "end"});
        await ctx.Response.WriteAsync("\n\n");
        await ctx.Response.Body.FlushAsync();
        
        while (!ct.IsCancellationRequested)
        {
            var outputMessageId = Guid.NewGuid().ToString();
            await ctx.Response.WriteAsync("data: ");
            await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = outputMessageId, Sender = "bot", Type = "start"});
            await ctx.Response.WriteAsync("\n\n");
            await foreach (var message in GetMessagesFromPython(incomeMessage, token))
            {
                await ctx.Response.WriteAsync("data: ");
                await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = message, Id = outputMessageId, Sender = "bot", Type = "stream"});
                await ctx.Response.WriteAsync("\n\n");
                await ctx.Response.Body.FlushAsync();
            }
            await ctx.Response.WriteAsync("data: ");
            await JsonSerializer.SerializeAsync(ctx.Response.Body, new DataDto { Message = "", Id = outputMessageId, Sender = "bot", Type = "end"});
            await ctx.Response.WriteAsync("\n\n");
            await ctx.Response.Body.FlushAsync();
        }
    }
});

app.SubscribeSSEStream("/create-stream");

app.Run();

async IAsyncEnumerable<string> GetMessagesFromPython(string message, string token)
{
    var uri = new Uri("ws://localhost:8000/api/chat/");
    ClientWebSocket ws;
    if (dict.TryGetValue(token, out var value))
    {
        ws = value;
    }
    else
    {
        ws = new ClientWebSocket();
        ws.Options.KeepAliveInterval = new TimeSpan(0, 10, 0);
        await ws.ConnectAsync(uri, default);
        dict.Add(token, ws);
    }
    
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
        if (result.MessageType != WebSocketMessageType.Text)
            continue;

        var receivedString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        var income = JsonSerializer.Deserialize<PythonMessageDto>(receivedString);
        if (income?.Type is "end" or "error" or "info" && income?.Sender is not "you")
        {
            break;
        }
        
        if (income is { Type: "stream", Message: "" })
            continue;
        
        yield return income?.Message ?? string.Empty;
    }
}