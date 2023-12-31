﻿using Database.Entities;
using FileAPI.Repositories.Account;
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
        public static async Task<AccountDb?> CheckAuthorization(this IAccountRepository accountRepository,
            HttpRequest request)
        {
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                return await accountRepository.Authenticate(username, password);
            }
            catch
            {
                return null;
            }
        }
    }
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IAccountRepository _accountRepository;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAccountRepository accountRepository) : base(options, logger, encoder, clock)
        {
            _accountRepository = accountRepository;
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

                var account = await _accountRepository.CheckAuthorization(Context.Request);
                if (account != null)
                {
                    var claims = new[] { new Claim(ClaimTypes.Email, account.Login), new Claim(ClaimTypes.Role, account.Role.ToString()) };
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
