using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
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

#region Logs

var logsDirectory = Path.Combine("Logs");
if (!Directory.Exists(logsDirectory))
{
    Directory.CreateDirectory(logsDirectory);
}
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

var app = builder.Build();

app.UseCors("AllowAnyOrigin");

/*app.MapGet("/create-stream", async (HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
    ctx.Response.Headers.Append("Connection", "keep-alive");
    ctx.Response.Headers.Append("Cache-Control", "no-store");
    ctx.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    ctx.Response.Headers.Append("X-Accel-Buffering", "no");
    
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
});*/

app.MapGet("/send-text", async ([FromQuery]string incomeMessage, [FromQuery]string typeChat, HttpContext ctx, CancellationToken ct) =>
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
            await foreach (var message in GetMessagesFromPython(incomeMessage))
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
        if (result.MessageType != WebSocketMessageType.Text)
            continue;

        var receivedString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        var income = JsonSerializer.Deserialize<PythonMessageDto>(receivedString);
        if (income?.Type is "end" or "error" or "info")
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default);
            break;
        }
                
        yield return income?.Message ?? string.Empty;
    }
}