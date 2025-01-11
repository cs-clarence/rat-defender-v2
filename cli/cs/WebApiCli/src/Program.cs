using Common.DependencyInjection.Extensions;
using ConsoleAppFramework;
using Iam.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiCli.Commands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNpgsqlDataSourceProvider();
builder.Services.AddDbConnectionSource();
builder.Services.AddUnitOfWork();

var configuration = builder.Configuration;

builder.Services.AddIamInfrastructure(configuration).AddIamDomain();

var app = builder.Build();

var scope = app.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
var consoleApp = ConsoleApp.Create();
consoleApp.Add<DatabaseCommands>("db");
consoleApp.Add<CreateEntityCommands>("create");

await consoleApp.RunAsync(args);
