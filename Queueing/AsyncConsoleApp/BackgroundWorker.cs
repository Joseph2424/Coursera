namespace AsyncConsoleApp;

using System.Threading.Channels;

public class BackgroundWorker(Channel<WorkItem> queue)
{
    private readonly Channel<WorkItem> _queue = queue;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Background worker started.");

        await foreach (var item in _queue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                Console.WriteLine(
                    $"[{DateTime.Now:T}] Processing Item {item.Id}: {item.Description}"
                );

                // Simulate I/O work
                await Task.Delay(Random.Shared.Next(1000, 3000), cancellationToken);

                Console.WriteLine($"[{DateTime.Now:T}] Completed Item {item.Id}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Processing cancelled.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Item {item.Id}: {ex.Message}");
            }
        }

        Console.WriteLine("Background worker stopped.");
    }
}
