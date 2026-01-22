using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProgressSpinner : IDisposable
{
    private readonly bool _enabled;
    private readonly CancellationTokenSource _cts = new();
    private Task? _worker;
    private string _label = "";

    public ProgressSpinner(bool enabled)
    {
        _enabled = enabled;
    }

    public void Start(string label)
    {
        if (!_enabled)
            return;

        _label = label;
        _worker = Task.Run(async () =>
        {
            char[] frames = ['|', '/', '-', '\\'];
            int i = 0;
            while (!_cts.IsCancellationRequested)
            {
                Console.Write($"\r{_label} {frames[i++ % frames.Length]}");
                await Task.Delay(200, _cts.Token).ContinueWith(_ => { });
            }
        });
    }

    public void Dispose()
    {
        if (!_enabled)
            return;

        _cts.Cancel();
        try { _worker?.Wait(200); } catch { }
        Console.Write($"\r{_label} done.          \n");
    }
}
