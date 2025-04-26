using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using MyChatApp.Web.Services.Ingestion;

namespace MyChatApp.Web.Services;

public class PdfQueueService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<PdfQueueService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobPdfSource> _blobLogger;
    private readonly IServiceProvider _serviceProvider;

    public PdfQueueService(QueueServiceClient queueServiceClient, 
        ILogger<PdfQueueService> logger, 
        BlobServiceClient blobServiceClient, 
        ILogger<AzureBlobPdfSource> blobLogger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
        _blobLogger = blobLogger;
        _serviceProvider = serviceProvider;
        _queueClient = queueServiceClient.GetQueueClient("new-pdf-queue");
        _queueClient.CreateIfNotExists();
    }
    
    // read from the queue
    public async Task<string> DequeuePdfAsync()
    {
        _logger.LogInformation("Checking for new PDFs in queue at: {time}", DateTimeOffset.Now);
        
        var exists = await _queueClient.ExistsAsync();
        if (!exists)
        {
            _logger.LogWarning("Queue does not exist.");
            return null;
        }
        
        var response = await _queueClient.ReceiveMessageAsync();
        if (response.HasValue && response.Value?.MessageText.Length > 0)
        {
            _logger.LogInformation("The queue has a message; Message Id: {messageId}", response.Value.MessageId);
            
            var message = response.Value;
            await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            
            // convert base64 bytes to string
            var bytes = Convert.FromBase64String(message.MessageText);
            var blobName = System.Text.Encoding.UTF8.GetString(bytes);
            
            _logger.LogInformation("Got Blob {message} off the queue: ", blobName);
            return blobName;
        }
        return null;
    }

    protected async Task IngestBlob(string blobName, CancellationToken stoppingToken)
    {   
        _logger.LogInformation("Ingesting blob {blobName}", blobName);
        
        using var scope = _serviceProvider.CreateScope();
        var dataIngestor = scope.ServiceProvider.GetRequiredService<DataIngestor>();
        var source = new AzureBlobPdfSource(_blobServiceClient, "pdf", _blobLogger);
        await dataIngestor.IngestBlobAsync(source, blobName);
        
        _logger.LogInformation("Ingested blob {blobName}", blobName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is running at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var blobName = await DequeuePdfAsync();
            if (blobName != null)
            {
                await IngestBlob(blobName, stoppingToken);
            }
            await Task.Delay(10000, stoppingToken); 
        }
    }
}