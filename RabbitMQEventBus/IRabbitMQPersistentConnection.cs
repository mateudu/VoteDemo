using System;
using RabbitMQ.Client;

namespace VoteDemo.BuildingBlocks.RabbitMQEventBus
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
