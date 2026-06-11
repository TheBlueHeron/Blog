namespace LinkDotNet.Blog.Web.Features.Services.Files;

public class UploadConfiguration
{
    internal const string ConfigurationSection = "ImageStorage";

    public required string AuthenticationMode { get; init; }
    public string? ServiceUrl { get; init; }
    public string? ConnectionString { get; init; }
    public required string ContainerName { get; init; }
    public string? CdnEndpoint { get; init; }
    public bool IsCdnEnabled => !string.IsNullOrWhiteSpace(CdnEndpoint);
}
