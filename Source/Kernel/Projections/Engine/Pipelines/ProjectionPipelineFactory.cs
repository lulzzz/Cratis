// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aksio.Cratis.Events.Projections.Changes;
using Aksio.Cratis.Events.Projections.Definitions;
using Microsoft.Extensions.Logging;

namespace Aksio.Cratis.Events.Projections.Pipelines
{
    /// <summary>
    /// Represents an implementation of <see cref="IProjectionPipelineFactory"/>.
    /// </summary>
    public class ProjectionPipelineFactory : IProjectionPipelineFactory
    {
        readonly IProjectionResultStores _projectionResultStores;
        readonly IProjectionEventProviders _projectionEventProviders;
        readonly IProjectionPositions _projectionPositions;
        readonly IChangesetStorage _changesetStorage;
        readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionPipelineFactory"/> class.
        /// </summary>
        /// <param name="projectionResultStores"><see cref="IProjectionResultStores"/> in the system.</param>
        /// <param name="projectionEventProviders"><see cref="IProjectionEventProviders"/> in the system.</param>
        /// <param name="projectionPositions"><see cref="IProjectionPositions"/> to use.</param>
        /// <param name="changesetStorage"><see cref="IChangesetStorage"/> for storing changesets as they occur.</param>
        /// <param name="loggerFactory"><see cref="ILoggerFactory"/> for creating loggers.</param>
        public ProjectionPipelineFactory(
            IProjectionResultStores projectionResultStores,
            IProjectionEventProviders projectionEventProviders,
            IProjectionPositions projectionPositions,
            IChangesetStorage changesetStorage,
            ILoggerFactory loggerFactory)
        {
            _projectionResultStores = projectionResultStores;
            _projectionEventProviders = projectionEventProviders;
            _projectionPositions = projectionPositions;
            _changesetStorage = changesetStorage;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IProjectionPipeline CreateFrom(IProjection projection, ProjectionPipelineDefinition definition)
        {
            var eventProvider = _projectionEventProviders.GetForType(definition.ProjectionEventProviderTypeId);
            var handler = new ProjectionPipelineHandler(_projectionPositions, _changesetStorage, _loggerFactory.CreateLogger<ProjectionPipelineHandler>());
            var pipeline = new ProjectionPipeline(
                projection,
                eventProvider,
                handler,
                new ProjectionPipelineJobs(_projectionPositions, eventProvider, handler, _loggerFactory),
                _loggerFactory.CreateLogger<ProjectionPipeline>());

            foreach (var resultStoreDefinition in definition.ResultStores)
            {
                var resultStore = _projectionResultStores.GetForTypeAndModel(resultStoreDefinition.TypeId, projection.Model);
                pipeline.StoreIn(resultStoreDefinition.ConfigurationId, resultStore);
            }
            return pipeline;
        }
    }
}