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
        //private bool _isStopping = false;
        private readonly TaskCompletionSource _src;
        private ConcurrentQueue<int> _testQueue;
        public Sports(ILogger<Sports> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _src = new();
            _logger = logger;

            _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(500)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = true
            });
            _testQueue = new ConcurrentQueue<int>();

            hostApplicationLifetime.ApplicationStarted.Register(appStarted);
            hostApplicationLifetime.ApplicationStopping.Register(appStopping);
            hostApplicationLifetime.ApplicationStopped.Register(appStopped);
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        private void appStarted()
        {
            _logger.LogInformation("LSports Started");
            _src.SetResult();
        }
        private void appStopping()
        {
            //_isStopping = true;
            _logger.LogInformation("LSports Stopping");
        }

        private void appStopped()
        {
            _logger.LogInformation("LSports Stopped");
        }

        public async Task RunLogic(CancellationToken cancellationToken)
        {
            await _src.Task;

            var p = Producer(_channel.Writer, cancellationToken);
            await Task.Delay(2000);
            var c = Consumer(_channel.Reader, cancellationToken);

            await Task.WhenAll(p, c);

            _hostApplicationLifetime.StopApplication();
        }

        private async Task Producer(ChannelWriter<int> writer, CancellationToken cancellationToken)
        {
            try
            {
                for (var i = 1; i < int.MaxValue; i++)
                {
                    await writer.WriteAsync(i, cancellationToken);
                    _testQueue.Enqueue(i);
                    //await Task.Delay(TimeSpan.FromSeconds(0.2), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            finally
            {
                writer.Complete();
                if (_testQueue.TryPeek(out int val))
                {
                    _logger.LogInformation($"peek val:{val},last val:{_testQueue.Last()}");
                }
                else
                {
                    _logger.LogInformation("TestQueue is empty");
                }
            }
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

                //await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            }
        }
    }
}
