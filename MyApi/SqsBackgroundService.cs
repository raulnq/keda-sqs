using Amazon.SQS.Model;
using Amazon.SQS;
namespace MyApi;

public class SqsBackgroundService : BackgroundService
{
    private readonly string _queueUrl;
    private readonly IAmazonSQS _sqsClient;

    public SqsBackgroundService(IConfiguration configuration, IAmazonSQS sqsClient)
    {
        _sqsClient = sqsClient;
        _queueUrl = configuration.GetValue<string>("QueueUrl");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Starting polling queue at {_queueUrl}");
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 5
            };
            var response = await _sqsClient.ReceiveMessageAsync(request);
            if (response.Messages.Any())
            {
                foreach (var msg in response.Messages)
                {
                    await Task.Delay(Random.Shared.Next(1000, 2500));
                    Console.WriteLine($"Message received with body {msg.Body}");
                    await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        ReceiptHandle = msg.ReceiptHandle
                    });
                }
            }
            else
            {
                Console.WriteLine("No message available");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}