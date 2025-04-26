using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace MyChatApp.Web.Services.Ingestion;

public class AzureBlobPdfSource : IIngestionSource
{
    private readonly BlobContainerClient _containerClient;
    public string SourceId { get; }
    private readonly string _containerName;
    private readonly ILogger<AzureBlobPdfSource> _logger;

    public AzureBlobPdfSource(BlobServiceClient blobServiceClient, string containerName, ILogger<AzureBlobPdfSource> logger)
    {
        // _containerClient = containerClient;
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
        _containerName = containerName;
        SourceId = $"AzureBlobPdfSource:{containerName}";
        _logger = logger;
        _logger.LogInformation("AzureBlobPdfSource initialized with container: {containerName}", containerName);
    }

    public async Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var results = new List<IngestedDocument>();
        await foreach (var blob in _containerClient.GetBlobsAsync())
        {
            if (!blob.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) continue;
            _logger.LogInformation("Getting new or modified document: {blobName}", blob.Name);
            
            var version = blob.Properties.LastModified?.UtcDateTime.ToString("o") ?? "unknown";
            var existing = await existingDocuments.FirstOrDefaultAsync(d => d.SourceId == SourceId && d.Id == blob.Name);
            if (existing == null)
            {
                results.Add(new IngestedDocument { Id = blob.Name, Version = version, SourceId = SourceId });
            }
            else if (existing.Version != version)
            {
                existing.Version = version;
                results.Add(existing);
            }
        }
        return results;
    }

    public async Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var blobNames = new HashSet<string>();
        await foreach (var blob in _containerClient.GetBlobsAsync())
        {
            if (blob.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                blobNames.Add(blob.Name);
        }
        return await existingDocuments.Where(d => d.SourceId == SourceId && !blobNames.Contains(d.Id)).ToListAsync();
    }

    public async Task<IEnumerable<SemanticSearchRecord>> CreateRecordsForDocumentAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, string documentId)
    {
        var blobClient = _containerClient.GetBlobClient(documentId);
        using var stream = await blobClient.OpenReadAsync();
        using var pdf = PdfDocument.Open(stream);
        var paragraphs = pdf.GetPages().SelectMany(GetPageParagraphs).ToList();
        var embeddings = await embeddingGenerator.GenerateAsync(paragraphs.Select(c => c.Text));
        return paragraphs.Zip(embeddings).Select((pair, index) => new SemanticSearchRecord
        {
            Key = $"{Path.GetFileNameWithoutExtension(documentId)}_{pair.First.PageNumber}_{pair.First.IndexOnPage}",
            FileName = documentId,
            PageNumber = pair.First.PageNumber,
            Text = pair.First.Text,
            Vector = pair.Second.Vector,
        });
    }

    // New method: process a PDF from a blob URL
    public async Task<IEnumerable<SemanticSearchRecord>> ProcessBlobUrlAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, string blobUrl)
    {
        var blobClient = new BlobClient(new Uri(blobUrl));
        using var stream = await blobClient.OpenReadAsync();
        using var pdf = PdfDocument.Open(stream);
        var paragraphs = pdf.GetPages().SelectMany(GetPageParagraphs).ToList();
        var embeddings = await embeddingGenerator.GenerateAsync(paragraphs.Select(c => c.Text));
        return paragraphs.Zip(embeddings).Select((pair, index) => new SemanticSearchRecord
        {
            Key = $"{Path.GetFileNameWithoutExtension(blobUrl)}_{pair.First.PageNumber}_{pair.First.IndexOnPage}",
            FileName = blobUrl,
            PageNumber = pair.First.PageNumber,
            Text = pair.First.Text,
            Vector = pair.Second.Vector,
        });
    }

    private static IEnumerable<(int PageNumber, int IndexOnPage, string Text)> GetPageParagraphs(Page pdfPage)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var pageText = string.Join(Environment.NewLine + Environment.NewLine,
            textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));
#pragma warning disable SKEXP0050
        return TextChunker.SplitPlainTextParagraphs([pageText], 200)
            .Select((text, index) => (pdfPage.Number, index, text));
#pragma warning restore SKEXP0050
    }
}
