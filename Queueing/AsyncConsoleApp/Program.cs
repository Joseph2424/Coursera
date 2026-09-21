using System.Threading.Channels;
using AsyncConsoleApp;

var channel = Channel.CreateUnbounded<WorkItem>();

using var cts = new CancellationTokenSource();

var worker = new BackgroundWorker(channel);

// Start worker
var workerTask = worker.StartAsync(cts.Token);

// Simulate multiple producers
var producerTasks = Enumerable.Range(1, 3)
    .Select(producerId => ProduceWorkAsync(
        producerId,
        channel.Writer,
        cts.Token))
    .ToArray();

await Task.WhenAll(producerTasks);

// Signal no more items
channel.Writer.Complete();

// Wait for queue to drain
await workerTask;

Console.WriteLine("All work completed.");

static async Task ProduceWorkAsync(
    int producerId,
    ChannelWriter<WorkItem> writer,
    CancellationToken cancellationToken)
{
    for (int i = 1; i <= 5; i++)
    {
        var item = new WorkItem(
            Id: producerId * 100 + i,
            Description: $"Produced by Producer {producerId}",
            CreatedAt: DateTime.UtcNow);

        await writer.WriteAsync(item, cancellationToken);

        Console.WriteLine(
            $"[{DateTime.Now:T}] Producer {producerId} queued Item {item.Id}");

        await Task.Delay(
            Random.Shared.Next(500, 1500),
            cancellationToken);
    }
}