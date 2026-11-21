// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");


// Builder
var builder = Host.CreateApplicationBuilder(args);




builder.Services.AddHostedService<Bootloader>();

// Build
var host = builder.Build();

// Run
host.Run();


