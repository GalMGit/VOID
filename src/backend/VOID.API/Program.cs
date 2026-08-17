using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using VOID.API.DI;
using VOID.API.EndpointsConfig;
using VOID.API.Middlewares;
using VOID.Application.UseCases.Messages.Commands.Create;
using VOID.Infrastructure.SignalR.Handlers.Chats.Created;
using VOID.Persistence.Database.Context;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using ChatHub = VOID.Infrastructure.SignalR.ChatHub;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Host.UseWolverine(opt =>
{
    opt.Discovery.IncludeAssembly(typeof(CreateMessageCommand).Assembly);
    opt.Discovery.IncludeAssembly(typeof(ChatCreatedSignalRHandler).Assembly);
    opt.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
});

builder.Services.AddConfiguration(builder.Configuration);

builder.Services.AddSignalR(o =>
    o.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
    options.MemoryBufferThreshold = 20 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(3);
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<VoidDbContext>();
    await dbContext.Database.MigrateAsync();
}

var api = app.MapGroup("/api");
app.MapEndpoints(api);

app.MapOpenApi();
app.MapScalarApiReference("/docs",options =>
{
    options.WithTitle("VOIDApi")
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", auth =>
        {
            auth.Token = string.Empty;
            auth.Description = "Bearer Token";
        });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.Run();
