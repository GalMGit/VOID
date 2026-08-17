using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VOID.Application.Abstractions.IServices.IAuthServices;
using VOID.Application.Abstractions.IServices.ICacheServices;
using VOID.Application.Abstractions.IServices.IMailServices;
using VOID.Application.Abstractions.IServices.IMediaServices;
using VOID.Application.Abstractions.IServices.ISecurityServices;
using VOID.Application.Abstractions.IServices.ISignalRServices;
using VOID.Infrastructure.Auth;
using VOID.Infrastructure.Cache;
using VOID.Infrastructure.Email;
using VOID.Infrastructure.Security;
using VOID.Infrastructure.SignalR;
using VOID.Infrastructure.Storage;

namespace VOID.Infrastructure.DI;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(
            IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "Nothing_";
            });
            
            services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
            services.Configure<PublicStorageOptions>(configuration.GetSection(nameof(PublicStorageOptions)));
            services.Configure<PrivateStorageOptions>(configuration.GetSection(nameof(PrivateStorageOptions)));
            services.Configure<ApiOptions>(configuration.GetSection(nameof(ApiOptions)));
            
            services.AddSingleton<S3ClientFactory>();
            
            services.AddSingleton<IPublicStorage>(sp =>
            {
                var factory = sp.GetRequiredService<S3ClientFactory>();
                var options = sp.GetRequiredService<IOptions<PublicStorageOptions>>()
                    .Value;
                
                return new PublicStorage(
                    factory.Create(
                        options.ServiceUrl,
                        options.AccessKey,
                        options.SecretKey),
                    Options.Create(options));
            });
            
            services.AddSingleton<IPrivateStorage>(sp =>
            {
                var factory = sp.GetRequiredService<S3ClientFactory>();
                var options = sp.GetRequiredService<IOptions<PrivateStorageOptions>>()
                    .Value;
                
                return new PrivateStorage(
                    factory.Create(
                        options.ServiceUrl,
                        options.AccessKey,
                        options.SecretKey),
                    Options.Create(options));
            });
            
            services.AddSingleton<IEmailQueueService, EmailQueueService>();
            services.AddHostedService<BackgroundEmailService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IMediaUrlService, MediaUrlService>();
            
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            return services;
        }
    }
}