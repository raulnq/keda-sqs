using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using MyApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddHostedService<SqsBackgroundService>();
builder.Services.AddAWSService<IAmazonSQS>();

var queue = builder.Configuration.GetValue<string>("QueueUrl");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/send", async ([FromServices] IAmazonSQS sqsClient, [FromBody] Request request) =>
{
    for (int i = 0; i < request.Messages; i++)
    {
        var body = $"{Guid.NewGuid()}";
        var message = new SendMessageRequest(queue, body);
        await sqsClient.SendMessageAsync(message);
        Console.WriteLine($"Message sent with body {body}");
    }
});

app.Run();

public record Request(int Messages);