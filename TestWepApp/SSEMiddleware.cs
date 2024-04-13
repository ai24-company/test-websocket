using System.Text.Json;

namespace TestWepApp;

public class SSEMiddleware
{
    private readonly RequestDelegate _nextDelegate;

    public SSEMiddleware(RequestDelegate nextDelegate)
    {
        _nextDelegate = nextDelegate;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers["Accept"] == "text/event-stream")
        {
            context.Response.Headers.Append("Content-type", "text/event-stream");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            
            var id = Guid.NewGuid().ToString();
            
            /*var start = new DataDto { Message = "", Id = id, IsMe = false, Type = "start" };
            var stream = new DataDto { Message = "Welcome!", Id = id, IsMe = false, Type = "stream" };
            var end = new DataDto { Message = "", Id = id, IsMe = false, Type = "end" };*/
            /*await context.Response.WriteAsync("data:");
            await JsonSerializer.SerializeAsync(context.Response.Body, start);
            await context.Response.WriteAsync("\n\n");
            await context.Response.WriteAsync("data:");
            await JsonSerializer.SerializeAsync(context.Response.Body, stream);
            await context.Response.WriteAsync("\n\n");
            
            while (!context.RequestAborted.IsCancellationRequested)
            {
                await Task.Delay(2000);
            }
            await context.Response.WriteAsync("data:");
            await JsonSerializer.SerializeAsync(context.Response.Body, end);
            await context.Response.WriteAsync("\n\n");*/

            await context.Response.Body.FlushAsync();
        }
        else
        {
            await _nextDelegate(context);
        }
    }
}