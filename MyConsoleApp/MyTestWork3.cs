using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyConsoleApp.Service;
using MyConsoleApp.Model;
using EventBusMessages;
using Newtonsoft.Json;
using Open.ChannelExtensions;

namespace MyConsoleApp
{
    public class MyTestWork3 : BackgroundService
    {
        private readonly ILogger<MyTestWork3> _logger;
        private readonly EasyNetQRabbitGrabber _grabber;

        private readonly TaskCompletionSource _rabbitReceiveCompletionSource;

        private readonly Channel<PlaceOrderRequestMessage> _sourceChannel;
        private readonly IHostApplicationLifetime _appLifetime;
        public MyTestWork3(ILogger<MyTestWork3> logger, EasyNetQRabbitGrabber grabber, IHostApplicationLifetime appLifetime)
        {
            _sourceChannel = Channel.CreateBounded<PlaceOrderRequestMessage>(new BoundedChannelOptions(10)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            _rabbitReceiveCompletionSource = new();

            _logger = logger;
            _grabber = grabber;
            _appLifetime = appLifetime;

            _appLifetime.ApplicationStopping.Register(appStopping);
        }

        private void appStopping()
        {
            _logger.LogInformation("App Stopping");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = Consumer(_sourceChannel.Reader);

            var producer = _grabber.RabbitReceive(_sourceChannel.Writer, _rabbitReceiveCompletionSource, stoppingToken);

            await Task.WhenAll(producer, consumer);

        }

        private async Task Consumer(ChannelReader<PlaceOrderRequestMessage> _sourceChannelReader)
        {
            await foreach (var msgList in _sourceChannelReader.Batch(10).WithTimeout(TimeSpan.FromSeconds(0.5)).ReadAllAsync())
            {
                foreach (var msg in msgList)
                {
                    _logger.LogInformation($"Received message: {JsonConvert.SerializeObject(msg)}");
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                }
            }
        }


        //public override async Task StopAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation(
        //        "Consume Scoped Service Hosted Service is stopping.");

        //    await base.StopAsync(stoppingToken);
        //}
    }
}
