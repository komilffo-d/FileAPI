using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;

namespace FileAPI.Misc
{
    public static class Extensions
    {
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            services.AddScoped<FileRepository, FileRepository>();
            services.AddScoped<TokenRepository, TokenRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            return services;
        }
    }
}
