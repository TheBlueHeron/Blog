using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LinkDotNet.Blog.Web.Features.Services.Files;

public class NoopStorageService : IBlobService
{
    public Task<List<(string, string)>> GetFilesAsync() => Task.FromResult<List<(string, string)>>([]);

    public Task<string> UploadFileAsync(string fileName, Stream fileStream, UploadOptions options) => Task.FromResult("No Storage Service was configured. Nothing was uploaded");
}
