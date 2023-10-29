using FileAPI.Repositories.Account;
using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using FileAPI.Services;

namespace FileAPI.Misc
{
    public static class Extensions
    {
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            services.AddScoped<FileRepository, FileRepository>();
            services.AddScoped<TokenRepository, TokenRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<FileUploadService, FileUploadService>();
            services.AddSingleton<FileProgressContainerService, FileProgressContainerService>();

            return services;
        }
    }
}
