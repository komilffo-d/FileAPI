using Database.Entities;
using FileAPI.EntityDTO.Token;
using FileAPI.Misc.Authentication;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using FileAPI.Repositories.User;
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
        private readonly IUserRepository _userRepository;
        public TokenController(TokenRepository tokenRepository, FileRepository fileRepository, IUserRepository userRepository)
        {
            _tokenRepository = tokenRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<TokenDto>> createToken([FromBody] int[] idFiles)
        {
            var user = await _userRepository.CheckAuthorization(Request);
            var created = await _tokenRepository.Create(new TokenDb
            {
                TokenName = Guid.NewGuid(),
                UserId = user?.Id,
            });

            var current = _fileRepository.GetAll(f => idFiles.Contains<int>(f.Id), f => f.Id, null, int.MaxValue);
            created?.Files?.AddRange(current);
            await _tokenRepository.Update(created!.Id, created!);
            return Created("Success", new TokenDto(created.Id, created.TokenName));
        }
    }
}
