using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LinkDotNet.Blog.Web.Features.Services.Files;

public interface IBlobService
{
    /// <summary>
    /// Gets all uploaded files as list of tuples, containing file name and asset url.
    /// </summary>
    /// <returns>A list with all file names and asset urls</returns>
    Task<List<(string, string)>> GetFilesAsync();

    /// <summary>
    /// Uploads the given file and returns the asset url.
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    /// <param name="fileStream">The file contents as a <see cref="Stream"/></param>
    /// <param name="options">The <see cref="UploadOptions"/> to use</param>
    /// <returns>The asset url</returns>
    Task<string> UploadFileAsync(string fileName, Stream fileStream, UploadOptions options);
}
