using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace LinkDotNet.Blog.Web.Features.Services.Files;

/// <summary>
/// Handles uploading files to the Azure blob storage.
/// </summary>
public class AzureBlobStorageService : IBlobService
{
    #region Objects and variables

    private const string CACHEOPTIONS = "public, max-age=604800";
    private const string ERR_CONN = "ConnectionString must be set when using ConnectionString authentication mode";
    private const string ERR_SVC = "ServiceUrl must be set when using Default authentication mode";
    private const char SLASH = '/';

    private readonly IOptions<UploadConfiguration> azureBlobStorageConfiguration;

    #endregion

    #region Construction

    /// <summary>
    /// Creates a new <see cref="AzureBlobStorageService"/> using the given <see cref="UploadConfiguration"/>.
    /// </summary>
    /// <param name="azureBlobStorageConfiguration">The <see cref="IOptions{UploadConfiguration}"/> to use</param>
    public AzureBlobStorageService(IOptions<UploadConfiguration> azureBlobStorageConfiguration) => this.azureBlobStorageConfiguration = azureBlobStorageConfiguration;

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Gets all uploaded files as list of tuples, containing file name and asset url.
    /// </summary>
    /// <returns>A list with all file names and asset urls</returns>
    public async Task<List<(string, string)>> GetFilesAsync()
    {
        var results = new List<(string, string)>();
        var containerName = azureBlobStorageConfiguration.Value.ContainerName;
        var (rootContainer, subContainer) = SplitContainerName(containerName);
        var client = CreateClient(azureBlobStorageConfiguration.Value);
        var blobContainerClient = client.GetBlobContainerClient(rootContainer);
        var resultBlobs = blobContainerClient.GetBlobsAsync().AsPages();

        await foreach (Page<BlobItem> blobPage in resultBlobs)
        {
            foreach (var itemName in blobPage.Values.Select(b => b.Name))
            {
                var blobClient = blobContainerClient.GetBlobClient($"{subContainer}/{itemName}");
                results.Add((itemName, GetAssetUrl(blobClient.Uri.ToString(), azureBlobStorageConfiguration.Value)));
            }
        }
        return results;
    }

    /// <summary>
    /// Uploads the given file and returns the asset url.
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    /// <param name="fileStream">The file contents as a <see cref="Stream"/></param>
    /// <param name="options">The <see cref="UploadOptions"/> to use</param>
    /// <returns>The asset url</returns>
    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, UploadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var containerName = azureBlobStorageConfiguration.Value.ContainerName;
        var client = CreateClient(azureBlobStorageConfiguration.Value);

        var (rootContainer, subContainer) = SplitContainerName(containerName);
        var blobContainerClient = client.GetBlobContainerClient(rootContainer);
        var blobClient = blobContainerClient.GetBlobClient($"{subContainer}/{fileName}");

        var blobOptions = new BlobUploadOptions();
        if (options.SetCacheControlHeader)
        {
            blobOptions.HttpHeaders = new BlobHttpHeaders
            {
                CacheControl = CACHEOPTIONS
            };
        }

        await blobClient.UploadAsync(fileStream, blobOptions);
        return GetAssetUrl(blobClient.Uri.ToString(), azureBlobStorageConfiguration.Value);
    }

    #endregion

    #region Private methods and functions

    /// <summary>
    /// Returns a new <see cref="BlobServiceClient"/> using the given <see cref="UploadConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="UploadConfiguration"/> to use</param>
    /// <returns>A <see cref="BlobServiceClient"/></returns>
    /// <exception cref="InvalidOperationException">Invalid configuration</exception>
    private static BlobServiceClient CreateClient(UploadConfiguration configuration)
    {
        if (configuration.AuthenticationMode == AuthenticationMode.ConnectionString.Key)
        {
            var connectionString = configuration.ConnectionString ?? throw new InvalidOperationException(ERR_CONN);
            return new BlobServiceClient(connectionString);
        }

        var serviceUrl = configuration.ServiceUrl ?? throw new InvalidOperationException(ERR_SVC);
        return new BlobServiceClient(new Uri(serviceUrl), new DefaultAzureCredential());
    }

    /// <summary>
    /// Returns the download url for the file at the given blob url.
    /// </summary>
    /// <param name="blobUrl">The blob url of the uploaded file</param>
    /// <param name="config">The <see cref="UploadConfiguration"/> to use when using Cdn</param>
    /// <returns>The asset url</returns>
    private static string GetAssetUrl(string blobUrl, UploadConfiguration config)
    {
        if (!config.IsCdnEnabled)
        {
            return blobUrl;
        }

        var cdnEndpoint = config.CdnEndpoint!.TrimEnd(SLASH);
        var blobUri = new Uri(blobUrl);
        var path = blobUri.AbsolutePath;

        return $"{cdnEndpoint}{path}";
    }

    /// <summary>
    /// Splits the container name in its rootcontainer and subcontainer names.
    /// </summary>
    /// <param name="containerName">The full name of the container</param>
    /// <returns>A tuple containing the root and subcontainer names</returns>
    private static (string rootContainer, string subContainer) SplitContainerName(string containerName)
    {
        var containerNames = containerName.Split(SLASH, StringSplitOptions.RemoveEmptyEntries);

        if (containerNames.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        var rootContainer = containerNames[0];
        var subContainer = string.Join(SLASH, containerNames.Skip(1));
        return (rootContainer, subContainer);
    }

    #endregion
}
