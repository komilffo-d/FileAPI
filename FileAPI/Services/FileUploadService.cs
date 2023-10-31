using FileAPI.Misc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Serilog;
using static FileAPI.Misc.ProgressStream;

namespace FileAPI.Services
{
    public class FileUploadService
    {
        private Guid _filesIdentity;

        private DirectoryInfo _directory;

        private long _sizeRequest { get; set; }

        private float _uploadBytes { get; set; }

        private readonly FileProgressContainerService _fileService;

        public FileUploadService(FileProgressContainerService fileService)
        {
            _fileService = fileService;
        }
        public async Task<List<string>> UploadFileAsync(Stream fileStream, string contentType, long contentSize, string directory, Guid filesIdentity)
        {

            _directory = Tools.CreateRelativeDirectory(directory);
            _filesIdentity = filesIdentity;
            _sizeRequest = contentSize;

            var multipartReader = new MultipartReader(GetBoundary(MediaTypeHeaderValue.Parse(contentType)), fileStream);
            var sectionForm = await multipartReader.ReadNextSectionAsync();
            var filesUpload = new List<string>();

                while (sectionForm != null)
                {

                    var fileSection = sectionForm.AsFileSection();
                    if (fileSection != null)
                        filesUpload.Add(await SaveFileAsync(fileSection));
                    sectionForm = await multipartReader.ReadNextSectionAsync();
                }
                _fileService.Delete(_filesIdentity);


            return filesUpload;
        }

        private string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
                throw new InvalidDataException("Отсутствует ограничитель или тип данных не множественный!");

            return boundary;
        }
        private async Task<string> SaveFileAsync(FileMultipartSection fileSection)
        {

            var filePathAbsolute = Path.Combine(_directory.FullName, fileSection.FileName);
            await using FileStream stream = new FileStream(filePathAbsolute, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1024);
            await using ProgressStream progressStream = new ProgressStream(fileSection.FileStream!);

            progressStream.UpdateProgress += UpdateProgress!;
            await progressStream.CopyToAsync(stream);
            return Path.GetRelativePath(AppContext.BaseDirectory, filePathAbsolute);
        }
        private void UpdateProgress(object sender, ProgressEventArgs e)
        {
            _uploadBytes += e.Progress; 
            _fileService.Write(_filesIdentity, Math.Round(_uploadBytes / _sizeRequest * 100, MidpointRounding.ToPositiveInfinity));
        }

    }
}
