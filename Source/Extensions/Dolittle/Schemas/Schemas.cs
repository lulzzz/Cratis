// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

extern alias SDK;

using Cratis.Events.Schemas;
using Cratis.Schemas;

namespace Cratis.Extensions.Dolittle.Schemas
{
    public class Schemas : SDK::Cratis.Events.Schemas.Schemas
    {
        readonly ISchemaStore _schemaStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schemas"/> class.
        /// </summary>
        /// <param name="eventTypes"><see cref="SDK::Cratis.Events.IEventTypes"/> to use.</param>
        /// <param name="schemaGenerator"><see cref="IJsonSchemaGenerator"/> for generating JSON schemas..</param>
        /// <param name="schemaStore">The underlying <see cref="ISchemaStore"/>.</param>
        public Schemas(
            SDK::Cratis.Events.IEventTypes eventTypes,
            IJsonSchemaGenerator schemaGenerator,
            ISchemaStore schemaStore) : base(eventTypes, schemaGenerator)
        {
            _schemaStore = schemaStore;
        }

        /// <inheritdoc/>
        public override void RegisterAll()
        {
            foreach (var schemaDefinition in _definitions)
            {
                _schemaStore.Register(
                    new Events.EventType(schemaDefinition.Type.EventTypeId.Value, schemaDefinition.Type.Generation.Value),
                    schemaDefinition.FriendlyName,
                    schemaDefinition.Schema);
            }
        }
    }
}