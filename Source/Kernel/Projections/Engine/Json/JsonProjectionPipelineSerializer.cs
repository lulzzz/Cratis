// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Aksio.Cratis.Events.Projections.Definitions;
using Aksio.Cratis.Json;

namespace Aksio.Cratis.Events.Projections.Json;

/// <summary>
/// Represents an implementation of <see cref="IJsonProjectionSerializer"/>.
/// </summary>
public class JsonProjectionPipelineSerializer : IJsonProjectionPipelineSerializer
{
    readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonProjectionPipelineSerializer"/>.
    /// </summary>
    public JsonProjectionPipelineSerializer()
    {
        _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new ConceptAsJsonConverterFactory()
                }
        };
    }

    /// <inheritdoc/>
    public string Serialize(ProjectionPipelineDefinition definition) => JsonSerializer.Serialize(definition, _serializerOptions);

    /// <inheritdoc/>
    public ProjectionPipelineDefinition Deserialize(string json) => JsonSerializer.Deserialize<ProjectionPipelineDefinition>(json, _serializerOptions)!;
}
