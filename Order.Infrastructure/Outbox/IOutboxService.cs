using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.Outbox
{
    public interface IOutboxService
    {
        Task SaveMessageAsync(string aggregateId, string eventType, string payload);
        Task SaveMessagesAsync(IEnumerable<OutboxMessage> messages);
        Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken);
        Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken cancellationToken);
        Task MarkAsFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken);
    }
}
