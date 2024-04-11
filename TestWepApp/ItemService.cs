namespace TestWepApp;

public class ItemService
{
    private TaskCompletionSource<Item?> _tcs = new();
    private long _id = 0;
    
    public void Reset()
    {
        _tcs = new TaskCompletionSource<Item?>();
    }

    public void NotifyNewItemAvailable()
    {
        _tcs.TrySetResult(new Item($"New Item {_id++}", Guid.NewGuid().ToString(), false));
    }

    public Task<Item?> WaitForNewItem()
    {
        // Simulate some delay in Item arrival
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(0, 29)));
            NotifyNewItemAvailable();
        });
        
        return _tcs.Task;
    }
}

public record Item(string message, string id, bool isMe);