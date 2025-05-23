using Common.AspNetCore.Middlewares;
using Common.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Server.Extension;
using RatDefender.AspNetCore.Api;
using RatDefender.DependencyInjection;
using RatDefender.Infrastructure.Persistence.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1");

builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddOutputCache(o =>
{
    o.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddProblemDetails();
builder.Services.AddUnitOfWork();
builder.Services.AddRatDefender(builder.Configuration);
builder.Services.AddTaskQueueBackgroundService();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler();

app.UseMiddleware<DomainExceptionHandlerMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<RatDefenderDbContext>();

    var conn = dbContext.Database.GetDbConnection();
    await conn.OpenAsync();
    await using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "PRAGMA journal_mode=WAL";
        await cmd.ExecuteNonQueryAsync();
    }
}


// Configure the HTTP request pipeline.
app.MapOpenApi()
    .CacheOutput()
    .AllowAnonymous();

app.MapScalarApiReference(o => { o.EndpointPathPrefix = "/scalar"; });

var api = app.MapGroup("/api");
api.MapRatDefenderApi();
app.MapGet("/health", () => new { Status = "OK" });

await app.RunAsync();