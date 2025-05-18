using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Order.Infrastructure.Repositories;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;


namespace Order.Infrastructure.Outbox
{
    public class RavenDbOutboxService : IOutboxService
    {

        private readonly RavenDbContext _dbContext;
        private readonly ILogger<RavenDbOutboxService> _logger;

        public RavenDbOutboxService(RavenDbContext dbContext, ILogger<RavenDbOutboxService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveMessageAsync(string aggregateId, string eventType, string payload)
        {
            var message = new OutboxMessage(aggregateId, eventType, payload);
            
            using var session = _dbContext.OpenAsyncSession();
            await session.StoreAsync(message);
            await session.SaveChangesAsync();
            
            _logger.LogInformation("Saved outbox message {MessageId} of type {EventType} for aggregate {AggregateId}", 
                message.Id, eventType, aggregateId);
        }

        public async Task SaveMessagesAsync(IEnumerable<OutboxMessage> messages)
        {
            using var session = _dbContext.OpenAsyncSession();
            
            foreach (var message in messages)
            {
                await session.StoreAsync(message);
            }
            
            await session.SaveChangesAsync();
            
            _logger.LogInformation("Saved {Count} outbox messages", messages.Count());
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
        {
            using var session = _dbContext.OpenAsyncSession();
            var messages = await session.Query<OutboxMessage>()
                .Where(m => m.Status == OutboxStatus.Pending)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
            
            return messages;
        }

        public async Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            using var session = _dbContext.OpenAsyncSession();
            
            var storedMessage = await session.LoadAsync<OutboxMessage>(message.Id, cancellationToken);
            if (storedMessage != null)
            {
                storedMessage.Status = OutboxStatus.Processed;
                storedMessage.ProcessedAt = DateTime.UtcNow;
                await session.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Marked message {MessageId} as processed", message.Id);
            }
        }

        public async Task MarkAsFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken)
        {
            using var session = _dbContext.OpenAsyncSession();
            
            var storedMessage = await session.LoadAsync<OutboxMessage>(message.Id, cancellationToken);
            if (storedMessage != null)
            {
                storedMessage.RetryCount++;
                storedMessage.Status = storedMessage.RetryCount >= 5 ? OutboxStatus.Failed : OutboxStatus.Pending;
                await session.SaveChangesAsync(cancellationToken);
                
                _logger.LogWarning("Message {MessageId} processing failed ({RetryCount}/5): {Error}", 
                    message.Id, storedMessage.RetryCount, error);
            }
        }
    }
}
