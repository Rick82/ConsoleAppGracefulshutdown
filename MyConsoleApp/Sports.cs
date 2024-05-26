using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyConsoleApp
{
    public class Sports
    {
        private readonly ILogger<Sports> _logger;
        private readonly Channel<int> _channel;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private bool _isStopping = false;
        private ConcurrentQueue<int> _testQueue;
        public Sports(ILogger<Sports> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;

            hostApplicationLifetime.ApplicationStarted.Register(appStarted);
            hostApplicationLifetime.ApplicationStopping.Register(appStopping);
            hostApplicationLifetime.ApplicationStopped.Register(appStopped);

            _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = true
            });
            _testQueue = new ConcurrentQueue<int>();
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        private void appStarted()
        {
            _logger.LogInformation("LSports Started");
        }

        private void appStopping()
        {
            _isStopping = true;
            _logger.LogInformation("LSports Stopping");
        }

        private void appStopped()
        {
            _logger.LogInformation("LSports Stopped");
        }

        public Task RunLogic(CancellationToken cancellationToken)
        {
            var p = Producer(_channel.Writer);
            var c = Consumer(_channel.Reader, cancellationToken);

            Task.WaitAll(p, c);
            _hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        private async Task Producer(ChannelWriter<int> writer)
        {
            for (var i = 1; i < int.MaxValue; i++)
            {
                if (_isStopping)
                {
                    _testQueue.TryPeek(out int val);
                    _logger.LogInformation($"The Channel Current Val is {val}, Last Val is {_testQueue.Last()}");
                    break;
                }
                await writer.WriteAsync(i);
                _testQueue.Enqueue(i);
                //await Task.Delay(TimeSpan.FromSeconds(1));
            }
            writer.Complete();
        }

        private async Task Consumer(ChannelReader<int> reader, CancellationToken cancellationToken)
        {
            await foreach (var val in reader.ReadAllAsync(CancellationToken.None))
            {
                _logger.LogDebug($"Consumer Receiver Val: {val}");
                _testQueue.TryDequeue(out int testVal);
                if (val != testVal)
                {
                    throw new Exception("not the same");
                }

                //await Task.Delay(TimeSpan.FromSeconds(0.1), CancellationToken.None);
            }
        }
    }
}
