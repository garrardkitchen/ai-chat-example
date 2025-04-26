using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using MyChatApp.Web.Services.Ingestion;

namespace MyChatApp.Web.Services;

public class PdfQueueService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<PdfQueueService> _logger;
    // private readonly DataIngestor _dataIngestor;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobPdfSource> _blobLogger;

    public PdfQueueService(QueueServiceClient queueServiceClient, 
        ILogger<PdfQueueService> logger, 
        // DataIngestor dataIngestor, 
        BlobServiceClient blobServiceClient, 
        ILogger<AzureBlobPdfSource> blobLogger)
    {
        _logger = logger;
        // _dataIngestor = dataIngestor;
        _blobServiceClient = blobServiceClient;
        _blobLogger = blobLogger;
        _queueClient = queueServiceClient.GetQueueClient("new-pdf-queue");
        // _queueClient.CreateIfNotExists();
    }
    
    // read from the queue
    public async Task<string> DequeuePdfAsync()
    {
        var exists = await _queueClient.ExistsAsync();
        if (!exists)
        {
            _logger.LogWarning("Queue does not exist.");
            return null;
        }
        
        var response = await _queueClient.ReceiveMessageAsync();
        if (response.HasValue && response.Value?.MessageText.Length > 0)
        {
            var message = response.Value;
            await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            _logger.LogInformation("PDF Blob: {message}", message.MessageText);
            return message.MessageText;
        }
        return null;
    }

    protected async Task IngestBlob(string url, CancellationToken stoppingToken)
    {
        // await _dataIngestor.IngestDataAsync(new AzureBlobPdfSource(_blobServiceClient, "pdf", _blobLogger));
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background Service is running at: {time}", DateTimeOffset.Now);
            var blobUrl = await DequeuePdfAsync();
            if (blobUrl != null)
            {
                await IngestBlob(blobUrl, stoppingToken);
            }
            await Task.Delay(5000, stoppingToken); // Run every second
        }
    }
}