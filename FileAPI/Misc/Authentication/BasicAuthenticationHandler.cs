using Database.Entities;
using FileAPI.Repositories.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace FileAPI.Misc.Authentication
{
    public static class Extensions
    {
        public static async Task<UserDb?> CheckAuthorization(this IUserRepository userRepository,
            HttpRequest request)
        {
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                return await userRepository.Authenticate(username, password);
            }
            catch
            {
                return null;
            }
        }
    }
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository _userRepository;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserRepository userRepository) : base(options, logger, encoder, clock)
        {
            _userRepository = userRepository;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint is not null && endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Пропущен тег Authorization!");
            }
            try
            {

                var user = await _userRepository.CheckAuthorization(Context.Request);
                if (user != null)
                {
                    var claims = new[] { new Claim(ClaimTypes.Email, user.UserName) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    return AuthenticateResult.Fail("Неверные данные для входа!");
                }
            }
            catch (Exception)
            {

            }
            return AuthenticateResult.Fail("Ошибка аутентификации!");
        }
    }
}
