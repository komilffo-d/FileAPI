using Database.Entities;
using FileAPI.EntityDTO.File;
using FileAPI.Misc;
using FileAPI.Misc.Authentication;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using FileAPI.Repositories.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly TokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;
        private const string nameZipFile = "template.zip";
        private static List<FileStream> _currentFiles = new List<FileStream>();

        public FileController(FileRepository fileRepository, TokenRepository tokenRepository, IUserRepository userRepository)
        {
            _fileRepository = fileRepository;
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet("{id:int}", Name = nameof(GetFile), Order = 1)]
        public async Task<ActionResult?> GetFile([FromRoute] int id)
        {

            var fileDb = await _fileRepository.Get(f => f.Id == id);
            var fileStream = await Tools.GetFile(fileDb?.FileName!);
            if (fileStream is not null)
            {
                var user = await _userRepository.CheckAuthorization(Request);
                fileDb?.Users?.Add(user);
                await _fileRepository.Update(fileDb!.Id, fileDb);
                return File(fileStream!, MimeMapping.MimeUtility.GetMimeMapping(fileDb?.FileName), fileDb?.FileName);
            }

            else
                return NotFound();
        }
        //TODO: Реализовать назначение прогресса по каждому загружаемому и скачиваемому файлу
        /*                FileSystemWatcher watcher;*/
        [HttpGet(Name = nameof(GetFiles), Order = 1)]
        [AllowAnonymous]
        public async Task<ActionResult?> GetFiles([FromQuery] int[] idArray)
        {
            var filesPath = _fileRepository.GetAll(file => true, f => f.FileName, null, int.MaxValue);
            var zip = await Tools.CreateZip(filesPath.Select(f => f.FileName).ToArray(), nameZipFile);
            if (zip is not null)
                return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
            else
                return NotFound();
        }


        [HttpGet("token", Name = nameof(GetFilesByToken), Order = 2)]
        [Authorize]
        public async Task<ActionResult?> GetFilesByToken([FromQuery] Guid? identity)
        {
            if (identity is null || identity is not Guid)
                return BadRequest();
            var tokenDb = await _tokenRepository.Get(t => t.TokenName.Equals(identity));
            if (tokenDb is null || tokenDb.Used == true)
                return BadRequest("Токен не существует или уже использован");
            if (DateTime.Now - tokenDb.timeStamp > new TimeSpan(0, 0, 0, 0, 0, 0))
            {
                tokenDb.Used = true;
                return NotFound("Время действия токена прошло!");
            }

            await _tokenRepository.LoadCollection(tokenDb, t => t.Files!);
            if (tokenDb?.Files?.Count > 0)
            {
                string[] filesPath = tokenDb?.Files?.Select(f => f.FileName).ToArray()!;
                var zip = await Tools.CreateZip(filesPath, nameZipFile);
                if (zip is not null)
                {
                    tokenDb.Used = true;
                    await _tokenRepository.Update(tokenDb.Id, tokenDb);

                    var userDb = await _userRepository.CheckAuthorization(Request);
                    if (userDb is not null)
                    {
                        userDb?.Files?.AddRange(tokenDb?.Files!);
                        await _userRepository.Update(userDb!.Id, userDb);
                    }


                    return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
                }

                else
                    return BadRequest();
            }
            else
                return NotFound();

        }
        [Authorize]
        [HttpGet("history", Name = nameof(GetHistoryFiles), Order = 1)]
        public async Task<ActionResult<List<FileDto>>?> GetHistoryFiles()
        {


            var user = await _userRepository.CheckAuthorization(Request);
            if (user is null)
                return BadRequest();

            await _userRepository.LoadCollection(user, t => t.Files!);
            if (user?.Files?.Count == 0)
                return NotFound();
            return user?.Files?.Select(f => f!.AsDto()).ToList()!;

        }
        //Лимит на загруженные файлы - 1 GiB (Гигибайт)
        [HttpPost(Name = "UploadFile", Order = 1)]
        [AllowAnonymous]
        [RequestSizeLimit(1_024_000_000)]
        public async Task<ActionResult?> UploadFile([FromForm] List<FileUploadDto> filesUpload)
        {

            DirectoryInfo? directory = Tools.CreateRelativeDirectory("Files");
            if (directory is not null && directory.Exists)
            {
                filesUpload.ForEach(fileUpload =>
                {
                    using (FileStream stream = new FileStream(Path.Combine(directory.FullName, fileUpload.FileDetails.FileName), FileMode.Create))
                    {
                        fileUpload.FileDetails.CopyTo(stream);
                        var created = _fileRepository.Create(new FileDb
                        {
                            FileName = fileUpload.FileDetails.FileName,
                            FileType = fileUpload.FileType
                        });
                    }
                });


                return Created("123", null);
            }

            return null;


        }
    }
}
