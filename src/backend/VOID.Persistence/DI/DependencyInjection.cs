using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VOID.Application.Abstractions.IRepositories.IChatRepositories;
using VOID.Application.Abstractions.IRepositories.IGroupRepositories;
using VOID.Application.Abstractions.IRepositories.IImageRepositories;
using VOID.Application.Abstractions.IRepositories.IMessageRepositories;
using VOID.Application.Abstractions.IRepositories.IUserRepositories;
using VOID.Persistence.Database.Context;
using VOID.Persistence.Repositories.ChatRepositories;
using VOID.Persistence.Repositories.GroupRepositories;
using VOID.Persistence.Repositories.ImageRepositories;
using VOID.Persistence.Repositories.MessageRepositories;
using VOID.Persistence.Repositories.UserRepositories;

namespace VOID.Persistence.DI;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPersistence(
            IConfiguration configuration)
        {
            services.AddDbContext<VoidDbContext>(x =>
            {
                x.UseNpgsql(configuration.GetConnectionString("Postgres"));
            });
            
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            
            return services;
        }
    }
}

