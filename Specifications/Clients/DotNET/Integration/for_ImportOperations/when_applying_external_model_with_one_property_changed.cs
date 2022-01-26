// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aksio.Cratis.Events;

namespace Aksio.Cratis.Integration.for_ImportOperations
{
    public class when_applying_external_model_with_one_property_changed : given.one_property_changed
    {
        SomeEvent event_appended;

        void Establish()
        {
            event_log
                .Setup(_ => _.Append(IsAny<EventSourceId>(), IsAny<object>()))
                .Callback((EventSourceId eventSourceId, object @event) => event_appended = (@event as SomeEvent)!);
        }

        async Task Because() => await operations.Apply(incoming);

        [Fact] void should_have_one_event() => event_appended.ShouldNotBeNull();
    }
}