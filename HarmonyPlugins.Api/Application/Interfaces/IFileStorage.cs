namespace HarmonyPlugins.Api.Application.Interfaces;

public interface IFileStorage
{
    Task UploadAsync(string path, Stream content, CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
