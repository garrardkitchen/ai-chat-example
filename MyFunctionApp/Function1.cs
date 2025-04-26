using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MyFunctionApp
{
    public class Function1
    {
        private readonly BlobContainerClient _containerClient; // = blobServiceClient.GetBlobContainerClient("uploads");

        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _containerClient = blobServiceClient.GetBlobContainerClient("uploads");
            _logger.LogInformation("Function 1 started");
        }

        [Function(nameof(Function1))]
        public async Task Run([BlobTrigger("uploads/{name}", Connection = "blobs")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            // _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");
        }
        
    }
}
