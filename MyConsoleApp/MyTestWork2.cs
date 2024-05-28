using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyConsoleApp
{
    public class MyTestWork2 : BackgroundService
    {
        private readonly ILogger<MyTestWork2> _logger;
        //private readonly Sports _lsports;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public MyTestWork2(ILogger<MyTestWork2> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;

            _hostApplicationLifetime.ApplicationStarted.Register(appStarted);
            _hostApplicationLifetime.ApplicationStopping.Register(appStopping);
            _hostApplicationLifetime.ApplicationStopped.Register(appStopped);
        }

        private void appStarted()
        {
            _logger.LogInformation("AppStarted");
        }

        private void appStopping()
        {
            _logger.LogInformation("AppStopping");
        }

        private void appStopped()
        {
            _logger.LogInformation("AppStopped");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var result = await WaitForAppStartup(_hostApplicationLifetime, stoppingToken);

            if (!result)
            {
                return;
            }
            await Task.WhenAll(LogVal(stoppingToken));
        }


        private async Task LogVal(CancellationToken stoppingToken)
        {
            for (var i = 0; i < int.MaxValue; i++)
            {
                _logger.LogInformation($"Task1 Log Val: {i}");
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            }
        }


        private async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
        {
            var startedSource = new TaskCompletionSource();
            var cancelledSource = new TaskCompletionSource();

            using var reg1 = lifetime.ApplicationStarted.Register(()=>
            {
                startedSource.SetResult();
            });
            using var reg2 = stoppingToken.Register(()=>
            {
                cancelledSource.SetResult();
            });

            Task completedTask = await Task.WhenAny(
                startedSource.Task,
                cancelledSource.Task).ConfigureAwait(false);

            // If the completed tasks was the "app started" task, return true, otherwise false
            return completedTask == startedSource.Task;
        }
    }
}
