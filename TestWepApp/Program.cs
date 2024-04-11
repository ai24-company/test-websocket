using Microsoft.AspNetCore.Mvc;
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

app.MapPost("/send-text", async (MessageDto dto, HttpContext ctx, ItemService service, CancellationToken ct) =>
{
    ctx.Response.Headers.Add("Content-Type", "text/event-stream");
    ctx.Response.Headers.Add("Cache-Control", "no-cache");
    ctx.Response.Headers.Add("Connection", "keep-alive");
    
    while (!ct.IsCancellationRequested)
    {
        var item = await service.WaitForNewItem();
        
        await ctx.Response.WriteAsync($"your data: {dto.Message}");
        await ctx.Response.WriteAsync($"\n\n");
        await Task.Delay(100);
        await ctx.Response.WriteAsync($"data: ");
        await JsonSerializer.SerializeAsync(ctx.Response.Body, item);
        await ctx.Response.WriteAsync($"\n\n");
        await ctx.Response.Body.FlushAsync();
            
        service.Reset();
    }
});

app.MapGet("/", async (HttpContext ctx, ItemService service, CancellationToken ct) =>
{
    ctx.Response.Headers.Add("Content-Type", "text/event-stream");
    ctx.Response.Headers.Add("Cache-Control", "no-cache");
    
    await ctx.Response.WriteAsync($"data: ляля-тополя");
    await ctx.Response.Body.FlushAsync();
        
    service.Reset();
});

app.Run();
