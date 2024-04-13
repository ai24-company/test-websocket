namespace TestWepApp;

public static class SSEExtension
{
    public static IEndpointConventionBuilder SubscribeSSEStream(this IEndpointRouteBuilder builder, string pattern)
    {
        var appBuilder = builder.CreateApplicationBuilder().UseMiddleware<SSEMiddleware>();
        var rDelegate = appBuilder.Build();
        return builder.Map(pattern, rDelegate);
    }
}