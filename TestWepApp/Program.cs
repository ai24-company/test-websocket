using System.Text;
using System.Text.Json;
using TestWepApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
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

app.UseHttpsRedirection();
app.UseRouting();

app.MapGet("/", async (HttpContext ctx, ItemService service, CancellationToken ct) =>
{
    ctx.Response.Headers.Add("Content-Type", "text/event-stream");
    
    while (!ct.IsCancellationRequested)
    {
        var item = await service.WaitForNewItem();
        
        await ctx.Response.WriteAsync($"data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, item);
        await ctx.Response.WriteAsync($"\n\n");
        await ctx.Response.Body.FlushAsync();
            
        service.Reset();
    }
});

app.Run();
