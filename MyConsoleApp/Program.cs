// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using Microsoft.Extensions.DependencyInjection;
using MyConsoleApp;
using MyConsoleApp.Service;

var host = Host.CreateDefaultBuilder(args)
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
        builder.AddCommandLine(args);
    })
    .ConfigureServices(services =>
    {
        //services.AddHostedService<MyTestWork>();
        //services.AddHostedService<MyTestWork2>();
        services.AddHostedService<MyTestWork3>();
        services.AddSingleton<Sports>();
        services.AddSingleton<EasyNetQRabbitGrabber>();
    })
    .Build();

await host.StartAsync();


await host.WaitForShutdownAsync();



