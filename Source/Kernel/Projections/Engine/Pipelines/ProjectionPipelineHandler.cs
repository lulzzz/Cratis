// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Reactive.Subjects;
using Aksio.Cratis.Changes;
using Aksio.Cratis.Events.Projections.Changes;
using Aksio.Cratis.Events.Store;
using Aksio.Cratis.Execution;
using Microsoft.Extensions.Logging;

namespace Aksio.Cratis.Events.Projections.Pipelines
{
    /// <summary>
    /// Represents an implementation of <see cref="IProjectionPipelineHandler"/>.
    /// </summary>
    public class ProjectionPipelineHandler : IProjectionPipelineHandler
    {
        readonly ILogger<ProjectionPipelineHandler> _logger;
        readonly IProjectionPositions _projectionPositions;
        readonly IChangesetStorage _changesetStorage;
        readonly ConcurrentDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber> _positions = new();
        readonly ReplaySubject<IReadOnlyDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber>> _observablePositions = new(1);

        /// <inheritdoc/>
        public IObservable<IReadOnlyDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber>> Positions => _observablePositions;

        /// <inheritdoc/>
        public IReadOnlyDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber> CurrentPositions => new ReadOnlyDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber>(_positions);

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionPipelineHandler"/> class.
        /// </summary>
        /// <param name="projectionPositions"><see cref="IProjectionPositions"/> for managing positions for pipelines.</param>
        /// <param name="changesetStorage"><see cref="IChangesetStorage"/> for tracking changesets.</param>
        /// <param name="logger">Logger for logging.</param>
        public ProjectionPipelineHandler(
            IProjectionPositions projectionPositions,
            IChangesetStorage changesetStorage,
            ILogger<ProjectionPipelineHandler> logger)
        {
            _projectionPositions = projectionPositions;
            _changesetStorage = changesetStorage;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task InitializeFor(IProjectionPipeline pipeline, ProjectionResultStoreConfigurationId configurationId)
        {
            var offset = await _projectionPositions.GetFor(pipeline.Projection, configurationId);
            UpdatePositionFor(configurationId, offset);
        }

        /// <inheritdoc/>
        public async Task<EventLogSequenceNumber> Handle(AppendedEvent @event, IProjectionPipeline pipeline, IProjectionResultStore resultStore, ProjectionResultStoreConfigurationId configurationId)
        {
            _logger.HandlingEvent(@event.Metadata.SequenceNumber);
            try
            {
                var correlationId = CorrelationId.New();
                var changesets = new List<IChangeset<AppendedEvent, ExpandoObject>>();
                await HandleEventFor(pipeline.Projection, resultStore, @event, changesets);
                await _changesetStorage.Save(correlationId, changesets);
                var nextSequenceNumber = @event.Metadata.SequenceNumber + 1;
                await _projectionPositions.Save(pipeline.Projection, configurationId, nextSequenceNumber);
                UpdatePositionFor(configurationId, nextSequenceNumber);
                return nextSequenceNumber;
            }
            catch (Exception ex)
            {
                await pipeline.Suspend($"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return @event.Metadata.SequenceNumber;
            }
        }

        async Task HandleEventFor(IProjection projection, IProjectionResultStore resultStore, AppendedEvent @event, List<IChangeset<AppendedEvent, ExpandoObject>> changesets)
        {
            if (projection.Accepts(@event.Metadata.Type))
            {
                var keyResolver = projection.GetKeyResolverFor(@event.Metadata.Type);
                var key = keyResolver(@event);
                _logger.GettingInitialValues(@event.Metadata.SequenceNumber);
                var initialState = await resultStore.FindOrDefault(key);
                var changeset = new Changeset<AppendedEvent, ExpandoObject>(@event, initialState);
                changesets.Add(changeset);
                _logger.Projecting(@event.Metadata.SequenceNumber);
                projection.OnNext(@event, changeset);
                _logger.SavingResult(@event.Metadata.SequenceNumber);
                if (changeset.HasChanges)
                {
                    await resultStore.ApplyChanges(key, changeset);
                }
            }
            else
            {
                _logger.EventNotAccepted(@event.Metadata.SequenceNumber, projection.Name, projection.Path, @event.Metadata.Type);
            }

            foreach (var child in projection.ChildProjections)
            {
                await HandleEventFor(child, resultStore, @event, changesets);
            }
        }

        void UpdatePositionFor(ProjectionResultStoreConfigurationId configurationId, EventLogSequenceNumber offset)
        {
            _positions[configurationId] = offset;
            _observablePositions.OnNext(new ReadOnlyDictionary<ProjectionResultStoreConfigurationId, EventLogSequenceNumber>(_positions));
        }
    }
}