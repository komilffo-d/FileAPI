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
        public async static Task<TupleZip?> CreateZip(string[] filesPath, string nameArchive = "", string directory = "files")
        {
            if (filesPath.Length > 0)
            {
                using (ZipFile zipArchive = new ZipFile())
                {
                    foreach (string relativeFilePath in filesPath)
                    {
                        FileInfo file = new FileInfo(Path.Combine(AppContext.BaseDirectory, relativeFilePath));
                        if (file.Exists)
                            zipArchive.AddFile(Path.Combine(AppContext.BaseDirectory, relativeFilePath), directory);
                    }
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
    }
}
