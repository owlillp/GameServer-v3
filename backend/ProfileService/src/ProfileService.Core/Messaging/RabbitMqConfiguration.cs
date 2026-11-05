using Shared.IntegrationEvents.Identity;
using Shared.Messaging;
using Wolverine;
using Wolverine.RabbitMQ;

namespace ProfileService.Core.Messaging;

public static class RabbitMqConfiguration
{
    private const string ACCOUNT_EVENTS_QUEUE = "profile.account-events";

    extension(WolverineOptions options)
    {
        public void ConfigureRabbitMq(string connectionString)
        {
            options.UseRabbitMq(new Uri(connectionString))
                .AutoProvision()
                .EnableWolverineControlQueues()
                .UseQuorumQueues()
                .UsePlatformChannelDefaults()
                .DeclareExchange(IdentityEventRouting.EXCHANGE, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Topic;
                    exchange.IsDurable = true;
                });

            options.ConfigureAuthEventsListeners();
        }

        private void ConfigureAuthEventsListeners()
        {
            options.ListenToRabbitQueue(ACCOUNT_EVENTS_QUEUE, queue =>
                queue.BindExchange(IdentityEventRouting.EXCHANGE, IdentityEventRouting.RoutingKeys.ALL_ACCOUNT_EVENTS));
        }
    }
}
