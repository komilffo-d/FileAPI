using FileAPI.EntityDTO.File;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Serilog;
using static FileAPI.Misc.ProgressStream;

namespace FileAPI.Misc.File
{
    public class FileUploadService 
    {
        private  Guid _ququeIdentity;
        private DirectoryInfo _directory;
        private readonly FileResultContainerService fileService;
        private long _sizeRequest { get; set; }
        private float _uploadByte { get; set; }
        public FileUploadService(FileResultContainerService _fileService)
        {
            fileService = _fileService;
        }
        public async Task<List<string>> UploadFileAsync(Stream fileStream, string contentType, long contentSize, string directory, Guid ququeIdentity)
        {

            _directory = Tools.CreateRelativeDirectory(directory);
            _ququeIdentity = ququeIdentity;
            _sizeRequest = contentSize;

            var fileCount = 0;
            long totalSizeInBytes = 0;

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(contentType));
            var multipartReader = new MultipartReader(boundary, fileStream);
            var section = await multipartReader.ReadNextSectionAsync();
            var filesMy = new List<string>();
            var filePaths = new List<string>();
            var notUploadedFiles = new List<string>();
            using var g = new Timer(new TimerCallback(LogProgress!), null, 0, 20);
            while (section != null)
            {

                var fileSection = section.AsFileSection();

                if (fileSection != null)
                {

                    filesMy.Add(await SaveFileAsync(fileSection, filePaths, notUploadedFiles));
                    fileCount++;

                }

                section = await multipartReader.ReadNextSectionAsync();
            }
            fileService.Delete(_ququeIdentity);
            return filesMy;
        }

        private string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            return boundary;
        }
        private async Task<string> SaveFileAsync(FileMultipartSection fileSection, IList<string> filePaths, IList<string> notUploadedFiles)
        {

            var filePath = Path.Combine(_directory.FullName, fileSection?.FileName);

            await using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite , 1024);
            await using ProgressStream progressStream = new ProgressStream(fileSection.FileStream);

            progressStream.UpdateProgress += UpdateProgress!;
            await progressStream.CopyToAsync(stream);
            return filePath;
        }
        private void UpdateProgress(object sender, ProgressEventArgs e)
        {
            _uploadByte += e.Progress;
        }

        private void LogProgress(object obj)
        {
            fileService.Write(_ququeIdentity, Math.Round((_uploadByte / _sizeRequest) * 100, MidpointRounding.ToPositiveInfinity));
            /*            _producerService.SendMessage($"Файл загружен на {Math.Round((_uploadByte / _sizeRequest) * 100, MidpointRounding.ToPositiveInfinity)} %", _ququeIdentity.ToString());*/
            Log.Information($"Progress is {Math.Round((_uploadByte / _sizeRequest) * 100, MidpointRounding.ToPositiveInfinity)}%");

        }
    }
}
