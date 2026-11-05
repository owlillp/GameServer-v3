using Shared.IntegrationEvents.Identity;
using Shared.IntegrationEvents.Identity.Events;
using Shared.Messaging;
using Wolverine;
using Wolverine.RabbitMQ;

namespace AuthService.Core.Messaging;

public static class RabbitMqConfiguration
{
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

            options.ConfigureAuthEventPublishing();
        }

        private void ConfigureAuthEventPublishing()
        {
            options.PublishMessagesToRabbitMqExchange<AccountRegisteredEvent>(
                    IdentityEventRouting.EXCHANGE,
                    _ => IdentityEventRouting.RoutingKeys.ACCOUNT_REGISTERED)
                .UseDurableOutbox();

            options.PublishMessagesToRabbitMqExchange<AccountDeletedEvent>(
                    IdentityEventRouting.EXCHANGE,
                    _ => IdentityEventRouting.RoutingKeys.ACCOUNT_DELETED)
                .UseDurableOutbox();

            options.PublishMessagesToRabbitMqExchange<AccountBlockedEvent>(
                    IdentityEventRouting.EXCHANGE,
                    _ => IdentityEventRouting.RoutingKeys.ACCOUNT_BLOCKED)
                .UseDurableOutbox();
        }
    }
}
