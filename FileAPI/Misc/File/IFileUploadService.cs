using FileAPI.EntityDTO.File;
using Microsoft.AspNetCore.WebUtilities;

namespace FileAPI.Misc.File
{
    public interface IFileUploadService
    {
        Task<List<FileDto>> UploadFileAsync(Stream fileStream, string contentType, long contentSize);
        Task<long>  SaveFileAsync(FileMultipartSection fileSection, IList<string> filePaths, IList<string> notUploadedFiles);
    }
}
