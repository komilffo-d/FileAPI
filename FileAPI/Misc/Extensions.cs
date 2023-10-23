using FileAPI.Repositories.File;
using FileAPI.Repositories.Token;
using FileAPI.Repositories.User;

namespace FileAPI.Misc
{
    public static class Extensions
    {
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            services.AddScoped<FileRepository,FileRepository>();
            services.AddScoped<TokenRepository, TokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}
