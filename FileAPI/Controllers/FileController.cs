using Database.Entities;
using Database.Enums;
using Database.Reflection;
using FileAPI.EntityDTO.File;
using FileAPI.Misc;
using FileAPI.Misc.Authentication;
using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;
using System.Security.Claims;
using static FileAPI.Misc.ProgressStream;

namespace FileAPI.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly TokenRepository _tokenRepository;
        private readonly IAccountRepository _accountRepository;
        private const string nameZipFile = "template.zip";
        private static List<FileStream> _currentFiles = new List<FileStream>();

        public FileController(FileRepository fileRepository, TokenRepository tokenRepository, IAccountRepository accountRepository)
        {
            _fileRepository = fileRepository;
            _tokenRepository = tokenRepository;
            _accountRepository = accountRepository;
        }

        [Authorize]
        [HttpGet("{id:int}", Name = nameof(GetFile), Order = 1)]
        public async Task<ActionResult?> GetFile([FromRoute] int id)
        {

            var fileDb = await _fileRepository.Get(f => f.Id == id);
            if (fileDb is null)
                return NotFound("Такой файл не был загружен!");
            var fileStream = await Tools.GetFile(fileDb?.FileName!);
            if (fileStream is null) 
                return NotFound("Такой файл отсутсвует на сервере!");   

            var accountDb = await _accountRepository.CheckAuthorization(Request);
            await _fileRepository.LoadReference(fileDb!, f => f.Account);
            if (fileDb.Account.Id != accountDb.Id && !fileDb.Shared)
                return BadRequest("У вас нет прав на получение этого файла!");

            return File(fileStream!, MimeMapping.MimeUtility.GetMimeMapping(fileDb.FileName), fileDb.FileName);
        }
        //TODO: Реализовать назначение прогресса по каждому загружаемому и скачиваемому файлу

        [HttpGet(Name = nameof(GetFiles), Order = 1)]
        [AllowAnonymous]
        public async Task<ActionResult?> GetFiles([FromBody] int[] idFiles)
        {
            var filesPath = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.FileName, null, int.MaxValue);
            if (filesPath.Count() != idFiles.Length)
                return NotFound("Файл(ов) с указанным(и) идектификатором(ами) не найден(о)!");

            var zip = await Tools.CreateZip(filesPath.Select(f => f.FileName).ToArray(), nameZipFile);
            if (zip is null)
                return NotFound("Файлы отсутствуют на сервере!");
            return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
        }


        [HttpGet("token", Name = nameof(GetFilesByToken), Order = 2)]
        [Authorize]
        public async Task<ActionResult> GetFilesByToken([FromQuery] Guid? identity)
        {
            if (identity is null || identity is not Guid)
                return BadRequest("Неправильный формат токена!");
            var tokenDb = await _tokenRepository.Get(t => t.TokenName.Equals(identity));
            if (tokenDb is null || tokenDb.Used == true)
                return BadRequest("Токен не существует или уже использован");
            if (DateTime.Now.ToUniversalTime() - tokenDb.timeStamp > new TimeSpan(0, 0, 0, 0, 0, 0))
            {
                tokenDb.Used = true;
                await _tokenRepository.Update(tokenDb.Id, tokenDb);
                return NotFound("Время действия токена прошло!");
            }

            await _tokenRepository.LoadCollection(tokenDb, t => t.Files!);
            if (tokenDb?.Files?.Count > 0)
            {
                string[] filesPath = tokenDb?.Files?.Select(f => f.FileName).Distinct().ToArray()!;
                var zip = await Tools.CreateZip(filesPath, nameZipFile);
                if (zip is not null)
                {
                    tokenDb.Used = true;
                    await _tokenRepository.Update(tokenDb.Id, tokenDb);



                    return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
                }

                else
                    return NotFound("Файлы отсутствуют на сервере!");
            }
            else
                return BadRequest("У токена нет привязанных файлов!");

        }
        [Authorize]
        [HttpGet("history", Name = nameof(GetHistoryFiles), Order = 1)]
        public async Task<ActionResult<List<FileDto>>> GetHistoryFiles()
        {


            var account = await _accountRepository.CheckAuthorization(Request);
            await _accountRepository.LoadCollection(account, t => t.Files!);
            if (account?.Files?.Count == 0)
                return NotFound("У пользольвателя нет загруженных файлов!");
            return account?.Files?.Select(f => f!.AsDto()).ToList()!;

        }
        //Лимит на загруженные файлы - 1 GiB (Гигибайт)
        [HttpPost(Name = "UploadFile", Order = 1)]
        [Authorize]
        [RequestSizeLimit(1_024_000_000)]
        public async Task<ActionResult<List<FileDto>>?> UploadFile([FromForm] IFormFileCollection filesUpload)
        {
            // TODO: Долелать поток прогресса для файла
            var directory = Tools.CreateRelativeDirectory("Files");
            var filesDto = new List<FileDto>();
            var accountDb = await _accountRepository.CheckAuthorization(Request);
            foreach (var fileUpload in filesUpload)
            {
                var client = new WebClient();
               
                using (FileStream stream = new FileStream(Path.Combine(directory.FullName, fileUpload.FileName), FileMode.Create, FileAccess.Write))
                {
                    using (ProgressStream progressStream = new ProgressStream(fileUpload.OpenReadStream()))
                    {
                        progressStream.UpdateProgress += UpdateProgress!;
                        progressStream.CopyTo(stream);
                        var created = await _fileRepository.Create(new FileDb
                        {
                            FileName = Path.Combine(directory.Name, fileUpload.FileName),
                            FileType = MimeMapping.MimeUtility.GetMimeMapping(fileUpload.FileName),
                            AccountId = accountDb!.Id,
                            Shared = Request.HttpContext.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType).Value == EnumReflection.GetDescription<Role>(Role.ADMIN) ? true : false
                        });

                        filesDto.Add(new FileDto(created.Id, created.FileName, created.FileType));
                    }


                }
            }

            return Created(Tools.GetUrl(Request), filesDto);


        }
        private void UpdateProgress(object sender, ProgressEventArgs e)
        {

            Log.Debug($"Progress is {e.Progress * 100.0f}%");
        }
    }
}
