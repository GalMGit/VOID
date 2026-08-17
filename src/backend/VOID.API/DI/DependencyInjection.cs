using System.Reflection;
using FluentValidation;
using Microsoft.OpenApi;
using VOID.API.EndpointsConfig;
using VOID.API.Extensions;
using VOID.Application.DI;
using VOID.Infrastructure.DI;
using VOID.Persistence.DI;
using VOID.API.Validators.Auth;
using Wolverine;
using ConnectionManager = VOID.Infrastructure.SignalR.ConnectionManager;
using IConnectionManager = VOID.Application.Abstractions.IServices.ISignalRServices.IConnectionManager;

namespace VOID.API.DI;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddConfiguration(IConfiguration configuration)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    var bearerScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header
                    };

                    document.Components ??= new OpenApiComponents();
                    document.AddComponent("Bearer", bearerScheme);

                    var securityRequirement = new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    };

                    foreach (var operation in document.Paths.Values
                                .SelectMany(path => path.Operations!))
                    {
                        operation.Value.Security ??= [];
                        operation.Value.Security.Add(securityRequirement);
                    }

                    return Task.CompletedTask;
                });
            });
            
            services.AddAuth(configuration);
            services.AddEndpoints(Assembly.GetExecutingAssembly());

            services.AddPersistence(configuration);
            services.AddInfrastructure(configuration);
            services.AddApplication(configuration);
            
            services.AddValidatorsFromAssembly(typeof(RegisterUserValidator).Assembly);
            services.AddProblemDetails();
        }
    }
}
