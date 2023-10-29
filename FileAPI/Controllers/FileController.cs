using Database.Entities;
using Database.Enums;
using Database.Reflection;
using FileAPI.EntityDTO.File;
using FileAPI.Misc;
using FileAPI.Misc.Authentication;
using FileAPI.Misc.File;
using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize]
        [HttpGet("get/{id:int}", Name = nameof(GetFile), Order = 1)]
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
                return Forbid();

            return File(fileStream!, MimeMapping.MimeUtility.GetMimeMapping(fileDb.FileName), fileDb.FileName);
        }

        [HttpGet("get", Name = nameof(GetFiles), Order = 1)]
        [Authorize]
        public async Task<ActionResult?> GetFiles([FromBody] int[] idFiles)
        {
            var filesDb = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.FileName, null, int.MaxValue).ToList();
            var accountDb = await _accountRepository.CheckAuthorization(Request);
            foreach (var fileDb in filesDb)
            {
                await _fileRepository.LoadReference(fileDb!, f => f.Account);
                if (fileDb.Account.Id != accountDb.Id && !fileDb.Shared)
                    return Forbid();
            }
            if (filesDb.Count() != idFiles.Length)
                return NotFound("Файл(ов) с указанным(и) идектификатором(ами) не найден(о)!");

            var zip = await Tools.CreateZip(filesDb.Select(f => f.FileName).ToArray(), nameZipFile);
            if (zip is null)
                return NotFound("Файлы отсутствуют на сервере!");
            return File(zip.stream, MimeMapping.MimeUtility.GetMimeMapping(zip.nameZip), zip.nameZip);
        }


        [HttpGet("get/token", Name = nameof(GetFilesByToken), Order = 2)]
        [Authorize]
        public async Task<ActionResult> GetFilesByToken([FromQuery] Guid? identity)
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
        [HttpGet("get/history", Name = nameof(GetHistoryFiles), Order = 1)]
        public async Task<ActionResult<List<FileDto>>> GetHistoryFiles()
        {


            var account = await _accountRepository.CheckAuthorization(Request);
            await _accountRepository.LoadCollection(account, t => t.Files!);
            if (account?.Files?.Count == 0)
                return NotFound("У пользольвателя нет загруженных файлов!");
            return account?.Files?.Select(f => f!.AsDto()).ToList()!;

        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
        {
            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                var factories = context.ValueProviderFactories;

                factories.RemoveType<FormValueProviderFactory>();
                factories.RemoveType<FormFileValueProviderFactory>();
                factories.RemoveType<JQueryFormValueProviderFactory>();
            }
            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }
        //Лимит на загруженные файлы - 1 GiB (Гигибайт)
        [HttpPost("upload", Name = "UploadFile", Order = 1)]
        [Authorize]
        [RequestSizeLimit(1_024_000_000)]
        [DisableFormValueModelBinding]
        public async Task<ActionResult<List<FileDto>>?> UploadFile([Required][FromQuery] Guid queueIdentity, [FromServices] FileUploadService fileService)
        {
            var accountDb = await _accountRepository.CheckAuthorization(Request);
            var fileUploadSummary = await fileService.UploadFileAsync(HttpContext.Request.Body, Request.ContentType, HttpContext.Request.ContentLength ?? 0, "Files", queueIdentity);
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



            return Created(Tools.GetUrl(Request), filesDb.AsParallel().Select(f => new FileDto(f.Id, f.FileName, f.FileType)).ToList());


        }
        [HttpGet]
        [Route("progress")]
        public async Task<IActionResult> GetProgressFile([Required][FromQuery] Guid ququeIdentity, [FromServices] FileResultContainerService _fileService)
        {
            /*            var result = rabbitMQService.ReceiveMessage(ququeIdentity.ToString());
                        return _consumerService.ReadMessgaes();*/
            var percent = _fileService.Read(ququeIdentity);
            if (percent is not null)
                return Ok($"Файл загружен на {percent} %");
            else
                return NoContent();
        }


    }
}
