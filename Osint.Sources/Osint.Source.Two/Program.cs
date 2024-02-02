using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Osint.Source.Two;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddSingleton(new ActivitySource("Osint.Source.Two", "0.0.1"))
    .AddSingleton<MessageReceiver>()
    .AddHostedService<HostedService>();

using var host = builder.Build();

await host.RunAsync();