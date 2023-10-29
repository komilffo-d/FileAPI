using Ionic.Zip;

namespace FileAPI.Misc
{
    public static class Tools
    {


        /// <summary>
        /// Создание директории на основе относительного пути
        /// </summary>
        /// <param name="relativePath"></param>
        public static DirectoryInfo? CreateRelativeDirectory(string relativePath)
        {

            if (!Path.IsPathRooted(relativePath))
            {
                DirectoryInfo directory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, relativePath));
                try
                {
                    if (!directory.Exists)
                    {
                        directory.Create();

                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                return directory;

            }
            return null;
        }

        public async static Task<FileStream?> GetFile(string name)
        {
            FileInfo file = new FileInfo(Path.Combine(AppContext.BaseDirectory, name.ToString()!));
            if (file.Exists)
            {
                FileStream fileStream = await Task.Run(() =>
                {
                    FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                    stream.Position = 0;
                    return stream;
                });


                return fileStream;
            }
            return null;
        }
        public sealed class TupleZip
        {
            public string nameZip;

            public MemoryStream stream;
        }
        public async static Task<TupleZip?> CreateZip(string[] filesPath, string nameArchive, string? password = null)
        {
            if (filesPath.Length > 0)
            {
                using (ZipFile zipArchive = new ZipFile())
                {
                    zipArchive.AlternateEncoding = System.Text.Encoding.UTF8;
                    zipArchive.AlternateEncodingUsage = ZipOption.AsNecessary;
                    zipArchive.Password = password;
                    foreach (string relativeFilePath in filesPath)
                    {
                        FileInfo file = new FileInfo(Path.Combine(AppContext.BaseDirectory, relativeFilePath));
                        if (file.Exists)
                            zipArchive.AddFile(Path.Combine(AppContext.BaseDirectory, relativeFilePath), String.Empty);
                        else
                            break;
                    }
                    if (zipArchive.Count != filesPath.Length)
                        return null;

                    zipArchive.Comment = "Используйте пароль от учётной записи для получения доступа к архиву.";

                    if (zipArchive.Count > 0)
                    {
                        MemoryStream output = await Task.Run(() =>
                        {
                            MemoryStream stream = new MemoryStream();
                            zipArchive.Save(stream);
                            stream.Position = 0;
                            zipArchive.Name = nameArchive;
                            return stream;
                        });
                        return new TupleZip() { nameZip = zipArchive.Name, stream = output };
                    }
                    else
                        return null;


                }
            }
            return null;

        }
        public static string GetUrl(HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}{request.Path}";
        }
        public static async Task<byte[]> GetBytes(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
