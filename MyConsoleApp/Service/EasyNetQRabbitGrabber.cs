using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.DI;
using EventBusMessages;
using Microsoft.Extensions.Logging;
using MyConsoleApp.Model;
using Newtonsoft.Json;

namespace MyConsoleApp.Service
{
    public class EasyNetQRabbitGrabber
    {
        private readonly ILogger<EasyNetQRabbitGrabber> _logger;
        private readonly string _connectionString;
        public EasyNetQRabbitGrabber(ILogger<EasyNetQRabbitGrabber> logger)
        {
            _connectionString = "host=localhost;virtualHost=/;username=guest;password=guest;prefetchcount=1;timeout=10";
            _logger = logger;
        }

        public async Task RabbitReceive(ChannelWriter<PlaceOrderRequestMessage> _sourceChannelWriter, TaskCompletionSource completeSource, CancellationToken cancellationToken)
        {
            using var bus = RabbitHutch.CreateBus(_connectionString, s =>
            {
                s.Register<ITypeNameSerializer, EventBusTypeNameSerializer>();
            });

            cancellationToken.Register(() =>
            {
                bus.Dispose();
                completeSource.SetResult();
            });

            Task receiveTask = receiveTask = bus.SendReceive.ReceiveAsync<PlaceOrderRequestMessage>("test", msg =>
            {
                return Task.Run(async () =>
                {
                    await _sourceChannelWriter.WriteAsync(msg);
                }, cancellationToken);
            }, cancellationToken);

            await completeSource.Task.ConfigureAwait(false);
            await receiveTask;
            _sourceChannelWriter.Complete();
        }
    }
}
