using Database.Entities;
using FileAPI.EntityDTO.Token;
using FileAPI.Misc.Authentication;
using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers
{
    [Route("api/token")]
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

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<TokenDto>> createToken([FromBody] int[] idFiles)
        {
            var account = await _accountRepository.CheckAuthorization(Request);
            var created = await _tokenRepository.Create(new TokenDb
            {
                TokenName = Guid.NewGuid(),
                AccountId = account!.Id,

            });

            var current = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.Id, null, int.MaxValue);
            if (current.Count() != idFiles.Length)
                return NotFound("Файл(ов) с указанным(и) идектификатором(ами) не найден(о)!");

            created?.Files?.AddRange(current);


            await _tokenRepository.Update(created!.Id, created!);
            return Created("Токен успешно создан!", new TokenDto(created.Id, created.TokenName));
        }
    }
}
