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
    private readonly IServiceProvider _serviceProvider;


    public PdfQueueService(QueueServiceClient queueServiceClient, 
        ILogger<PdfQueueService> logger, 
        // DataIngestor dataIngestor, 
        BlobServiceClient blobServiceClient, 
        ILogger<AzureBlobPdfSource> blobLogger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        // _dataIngestor = dataIngestor;
        _blobServiceClient = blobServiceClient;
        _blobLogger = blobLogger;
        _queueClient = queueServiceClient.GetQueueClient("new-pdf-queue");
        _serviceProvider = serviceProvider;
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
            
            // convert base64 bytes to string
            var bytes = Convert.FromBase64String(message.MessageText);
            var blobName = System.Text.Encoding.UTF8.GetString(bytes);
            
            _logger.LogInformation("PDF Blob: {message}", blobName);
            return blobName;
        }
        return null;
    }

    protected async Task IngestBlob(string blobName, CancellationToken stoppingToken)
    {
        // await _dataIngestor.IngestDataAsync(new AzureBlobPdfSource(_blobServiceClient, "pdf", _blobLogger));
        using var scope = _serviceProvider.CreateScope();
        var dataIngestor = scope.ServiceProvider.GetRequiredService<DataIngestor>();
        var source = new AzureBlobPdfSource(_blobServiceClient, "pdf", _blobLogger);
        await dataIngestor.IngestBlobAsync(source, blobName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Background Service is running at: {time}", DateTimeOffset.Now);
            var blobName = await DequeuePdfAsync();
            if (blobName != null)
            {
                await IngestBlob(blobName, stoppingToken);
            }
            await Task.Delay(5000, stoppingToken); // Run every second
        }
    }
}