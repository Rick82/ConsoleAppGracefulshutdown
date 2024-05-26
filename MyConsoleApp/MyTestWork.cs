using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MyConsoleApp
{
    public class MyTestWork : BackgroundService
    {
        private readonly ILogger<MyTestWork> _logger;
        private readonly Sports _lsports;
        public MyTestWork(ILogger<MyTestWork> logger, Sports lsports)
        {
            _logger = logger;
            _lsports = lsports;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _lsports.RunLogic(stoppingToken);
        }
    }
}
