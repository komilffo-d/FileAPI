using Database.Entities;
using Database.Enums;
using Database.Reflection;
using FileAPI.EntityDTO.File;
using FileAPI.Misc;
using FileAPI.Misc.Authentication;
using FileAPI.Misc.File;
using FileAPI.Misc.File.Attributes;
using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using FileAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FileAPI.Controllers
{
    [ApiController]
    [Route("file")]
    public class FileController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly TokenRepository _tokenRepository;
        private readonly IAccountRepository _accountRepository;

        public FileController(FileRepository fileRepository, TokenRepository tokenRepository, IAccountRepository accountRepository)
        {
            _fileRepository = fileRepository;
            _tokenRepository = tokenRepository;
            _accountRepository = accountRepository;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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
            if (EnumReflection.GetDescription<Role>(accountDb.Role) != EnumReflection.GetDescription<Role>(Role.ADMIN))
            {
                await _fileRepository.LoadReference(fileDb!, f => f.Account);
                if (fileDb.Account.Id != accountDb.Id && !fileDb.Shared)
                    return Forbid();
            }


            return File(fileStream!, MimeMapping.MimeUtility.GetMimeMapping(fileDb.FileName), fileDb.FileName);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [HttpGet("", Name = nameof(GetFiles), Order = 1)]
        [Authorize]
        public async Task<ActionResult?> GetFiles([FromBody] int[] idFiles)
        {
            var filesDb = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.FileName, null, int.MaxValue).ToList();
            var accountDb = await _accountRepository.CheckAuthorization(Request);

            if (filesDb.Count() != idFiles.Length)
                return NotFound("Файл(ов) с указанным(и) идектификатором(ами) не найден(о)!");
            if (EnumReflection.GetDescription<Role>(accountDb.Role) != EnumReflection.GetDescription<Role>(Role.ADMIN))
                foreach (var fileDb in filesDb)
                {
                    await _fileRepository.LoadReference(fileDb!, f => f.Account);
                    if (fileDb.Account.Id != accountDb.Id && !fileDb.Shared)
                        return Forbid();
                }


            var zip = await Tools.CreateZip(filesDb.Select(f => f.FileName).ToArray(), FileSettings.nameZipFileDefault, accountDb.Password);
            if (zip is null)
                return NotFound("Файлы отсутствуют на сервере!");
            return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("token/{identity:guid}", Name = nameof(GetFilesByToken), Order = 2)]
        [Authorize]
        public async Task<ActionResult> GetFilesByToken([FromRoute] Guid? identity)
        {
            if (identity is null || identity is not Guid)
                return BadRequest("Неправильный формат токена!");
            var tokenDb = await _tokenRepository.Get(t => t.TokenName.Equals(identity));
            if (tokenDb is null || tokenDb.Used == true)
                return NotFound("Токен не существует или уже использован");
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
                var zip = await Tools.CreateZip(filesPath, FileSettings.nameZipFileDefault);
                if (zip is not null)
                {
                    tokenDb.Used = true;
                    await _tokenRepository.Update(tokenDb.Id, tokenDb);

                    return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
                }

                else
                    return NotFound("Файл(ы) отсутствуют на сервере!");
            }
            else
                return BadRequest("У токена нет привязанных файлов!");

        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
        [ProducesResponseType(201)]
        [ProducesResponseType(204)]
        [HttpPost("upload", Name = "UploadFile", Order = 1)]
        [Authorize]
        [RequestSizeLimit(1_024_000_000)]
        [DisableFormValueModelBinding]
        public async Task<ActionResult<List<FileDto>>?> UploadFile([Required][FromQuery] Guid filesIdentity, [FromServices] FileUploadService fileService)
        {
            var accountDb = await _accountRepository.CheckAuthorization(Request);

            var fileUploadSummary = await fileService.UploadFileAsync(HttpContext.Request.Body, Request.ContentType, HttpContext.Request.ContentLength ?? 0, FileSettings.directoryDefault, filesIdentity);

            if (fileUploadSummary.Count() == 0)
                return NoContent();

            var filesDb = fileUploadSummary.AsParallel().Select(fname =>
            {
                return new FileDb
                {
                    FileName = fname,
                    FileType = MimeMapping.MimeUtility.GetMimeMapping(fname),
                    AccountId = accountDb!.Id,
                    Shared = Request.HttpContext.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType).Value == EnumReflection.GetDescription<Role>(Role.ADMIN) ? true : false
                };
            }).ToList();

            filesDb.ForEach(f =>
            {
                _fileRepository.Create(f).Wait();
            });



            return Created(Tools.GetUrl(Request), filesDb.AsParallel().Select(f => f.AsDto()).ToList());


        }
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [HttpGet]
        [Authorize]
        [Route("progress")]
        public async Task<IActionResult> GetProgressFile([Required][FromQuery] Guid filesIdentity, [FromServices] FileProgressContainerService _fileService)
        {
            var percent = _fileService.Read(filesIdentity);
            if (percent is not null)
                return Ok($"Файл загружен на {percent} %");
            else
                return NoContent();
        }


    }
}
