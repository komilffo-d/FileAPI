using Database.Entities;
using Database.Enums;
using FileAPI.EntityDTO.Token;
using FileAPI.Misc.Authentication;
using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers
{
    [Route("token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly TokenRepository _tokenRepository;
        private readonly FileRepository _fileRepository;
        private readonly IAccountRepository _accountRepository;
        public TokenController(TokenRepository tokenRepository, FileRepository fileRepository, IAccountRepository accountRepository)
        {
            _tokenRepository = tokenRepository;
            _fileRepository = fileRepository;
            _accountRepository = accountRepository;
        }
        [ProducesResponseType(201)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<TokenDto>> createToken([FromBody] int[] idFiles)
        {
            var accountDb = await _accountRepository.CheckAuthorization(Request);

            var filesDb = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.Id, null, int.MaxValue).ToList();
            if (filesDb.Count() != idFiles.Length)
                return NotFound("Файл(ов) с указанным(и) идектификатором(ами) не найден(о)!");

            if (accountDb.Role != Role.ADMIN)
                foreach (var fileDb in filesDb)
                {
                    await _fileRepository.LoadReference(fileDb!, f => f.Account);
                    if (fileDb.Account.Id != accountDb.Id && !fileDb.Shared)
                        return Forbid();
                }
            var created = await _tokenRepository.Create(new TokenDb
            {
                TokenName = Guid.NewGuid(),
                AccountId = accountDb!.Id,

            });
            created?.Files?.AddRange(filesDb);


            await _tokenRepository.Update(created!.Id, created!);
            return Created("", new TokenDto(created.Id, created.TokenName));
        }
    }
}
