// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using Aksio.Cratis.DependencyInversion;
using Aksio.Cratis.Execution;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aksio.Cratis.Events.Store.MongoDB;

/// <summary>
/// Represents an implementation of <see cref="IEventSequences"/> for MongoDB.
/// </summary>
public class MongoDBEventSequences : IEventSequences
{
    readonly ILogger<MongoDBEventSequences> _logger;
    readonly IExecutionContextManager _executionContextManager;
    readonly ProviderFor<IEventStoreDatabase> _eventStoreDatabaseProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="MongoDBEventSequences"/>.
    /// </summary>
    /// <param name="executionContextManager"><see cref="IExecutionContextManager"/> for getting current <see cref="ExecutionContext"/>.</param>
    /// <param name="eventStoreDatabaseProvider"><see cref="ProviderFor{T}">Provider for</see> <see cref="IMongoDatabase"/>.</param>
    /// <param name="logger"><see cref="ILogger"/> for logging.</param>
    public MongoDBEventSequences(
        IExecutionContextManager executionContextManager,
        ProviderFor<IEventStoreDatabase> eventStoreDatabaseProvider,
        ILogger<MongoDBEventSequences> logger)
    {
        _eventStoreDatabaseProvider = eventStoreDatabaseProvider;
        _logger = logger;
        _executionContextManager = executionContextManager;
    }

    /// <inheritdoc/>
    public async Task Append(
        EventSequenceId eventSequenceId,
        EventSequenceNumber sequenceNumber,
        EventSourceId eventSourceId,
        EventType eventType,
        DateTimeOffset validFrom,
        JsonObject content)
    {
        try
        {
            _logger.Appending(sequenceNumber);
            var @event = new Event(
                sequenceNumber,
                _executionContextManager.Current.CorrelationId,
                _executionContextManager.Current.CausationId,
                _executionContextManager.Current.CausedBy,
                eventType.Id,
                DateTimeOffset.UtcNow,
                validFrom,
                eventSourceId,
                new Dictionary<string, BsonDocument>
                {
                        { eventType.Generation.ToString(), BsonDocument.Parse(content.ToJsonString()) }
                },
                Array.Empty<EventCompensation>());
            await GetCollectionFor(eventSequenceId).InsertOneAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.AppendFailure(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task Compensate(
        EventSequenceId eventSequenceId,
        EventSequenceNumber sequenceNumber,
        EventType eventType,
        DateTimeOffset validFrom,
        JsonObject content) => throw new NotImplementedException();

    IMongoCollection<Event> GetCollectionFor(EventSequenceId eventSequenceId) => _eventStoreDatabaseProvider().GetEventSequenceCollectionFor(eventSequenceId);
}
