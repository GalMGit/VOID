using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VOID.Infrastructure.Auth;

namespace VOID.API.Extensions;

public static class AuthExtension
{
    extension(IServiceCollection services)
    {
        public void AddAuth(IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

            services.AddOptions<JwtOptions>()
                .Validate(o => !string.IsNullOrEmpty(o.SecretKey), "SecretKey is required")
                .ValidateOnStart();

            var jwtOptions = configuration
                .GetSection(nameof(JwtOptions))
                .Get<JwtOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    o.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions!.SecretKey)),
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        NameClaimType = ClaimTypes.NameIdentifier,
                    };

                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var request = context.HttpContext.Request;
                            var isHubRequest = request.Path.StartsWithSegments("/chatHub",
                                StringComparison.OrdinalIgnoreCase);

                            if (isHubRequest)
                            {
                                var accessToken = request.Query["access_token"];

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                    return Task.CompletedTask;
                                }
                            }

                            var authHeader = request.Headers.Authorization.FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) &&
                                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authHeader["Bearer ".Length..].Trim();
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        }
    }
}