using Shared.Messaging;
using Wolverine;
using Wolverine.RabbitMQ;

namespace RealtimeService.Core.Messaging;

public static class RabbitMqConfiguration
{
    extension(WolverineOptions options)
    {
        // Подписки на доменные события добавляются сюда вместе с обработчиками
        // в Messaging/Handlers: очередь realtime.<domain>-events + BindExchange.
        public void ConfigureRabbitMq(string connectionString) =>
            options.UseRabbitMq(new Uri(connectionString))
                .AutoProvision()
                .EnableWolverineControlQueues()
                .UseQuorumQueues()
                .UsePlatformChannelDefaults();
    }
}
