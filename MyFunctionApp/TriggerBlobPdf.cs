using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MyFunctionApp;

public class TriggerBlobPdf
{
    private readonly ILogger<TriggerBlobPdf> _logger;

    public TriggerBlobPdf(ILogger<TriggerBlobPdf> logger)
    {
        _logger = logger;
    }

    [Function(nameof(TriggerBlobPdf))]
    [QueueOutput("new-pdf-queue")]
    public async Task<string> Run([BlobTrigger("pdf/{name}", Connection = "")] Stream stream, string name)
    {
        using var blobStreamReader = new StreamReader(stream);
        var content = await blobStreamReader.ReadToEndAsync();
        _logger.LogInformation($"C# Blob trigger function Processed PDF blob\n Name: {name}");
        // _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        return name;
    }
}