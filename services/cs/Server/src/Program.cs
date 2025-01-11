using Common.DependencyInjection.Extensions;
using Server.Extension;
using RatDefender.AspNetCore.Api;
using RatDefender.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1");

builder.Services.AddOutputCache(o =>
{
    o.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddUnitOfWork();
builder.Services.AddRatDefender(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi()
    .CacheOutput()
    .AllowAnonymous();

app.MapScalarApiReference(o =>
{
    o.EndpointPathPrefix = "/scalar";
});

app.MapGroup("/api").MapRatDefenderApi();

await app.RunAsync();