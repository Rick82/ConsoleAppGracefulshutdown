// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using Microsoft.Extensions.DependencyInjection;
using MyConsoleApp;

Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
    })
    .UseSerilog((hostContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
    })
    .ConfigureAppConfiguration((hostContext, builder) =>
    {
        var enviroment = hostContext.HostingEnvironment.EnvironmentName;
        builder.AddJsonFile("appsettings.json", false, true);
        builder.AddJsonFile($"appsettings.{enviroment}.json", true, true);
        builder.AddEnvironmentVariables();

    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<MyTestWork>();
        services.AddSingleton<Sports>();
    })
    .Build().RunAsync();





