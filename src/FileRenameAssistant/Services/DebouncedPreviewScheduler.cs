namespace FileRenameAssistant.Services;

public sealed class DebouncedPreviewScheduler
{
    private CancellationTokenSource? _cts;
    private readonly TimeSpan _delay;

    public DebouncedPreviewScheduler(TimeSpan? delay = null)
    {
        _delay = delay ?? TimeSpan.FromMilliseconds(250);
    }

    public void Schedule(Func<CancellationToken, Task> action)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_delay, token);
                if (!token.IsCancellationRequested)
                {
                    await action(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, token);
    }
}
