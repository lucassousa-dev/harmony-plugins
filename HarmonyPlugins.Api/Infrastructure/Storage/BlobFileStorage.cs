using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HarmonyPlugins.Api.Application.Interfaces;

namespace HarmonyPlugins.Api.Infrastructure.Storage;

// Arquivos dos plugins em contêiner privado do Azure Blob Storage (Azurite no ambiente local).
public class BlobFileStorage(IConfiguration configuration) : IFileStorage
{
    private readonly Lazy<BlobContainerClient> _container = new(() => new BlobContainerClient(
        configuration.GetConnectionString("BlobStorage")
            ?? throw new InvalidOperationException("A connection string BlobStorage não foi configurada."),
        configuration["BlobStorage:Container"] ?? "plugin-files"));

    public async Task UploadAsync(string path, Stream content, CancellationToken cancellationToken = default)
    {
        await _container.Value.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        await _container.Value.UploadBlobAsync(path, content, cancellationToken);
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        await _container.Value.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);
    }
}
