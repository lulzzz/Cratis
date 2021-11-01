// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Dynamic;
using Newtonsoft.Json.Schema;

namespace Cratis.Events.Projections.for_Projection
{
    public class when_next_event_is_of_interest : Specification
    {
        static EventType event_a = "05c2799e-e3ad-43b6-87bb-9fecb0b4e147";
        static EventType event_b = "4212376e-dd74-44f4-8ed4-1b7fe314d208";
        Projection projection;
        List<EventContext> observed_events;
        ExpandoObject initial_state;

        void Establish()
        {
            projection = new Projection(
                "0b7325dd-7a25-4681-9ab7-c387a6073547",
                new Model(string.Empty, new JSchema()),
                new[] {
                    new EventTypeWithKeyResolver(event_b, KeyResolvers.EventSourceId)
                });

            dynamic state = initial_state = new();
            state.Integer = 42;

            observed_events = new();
            projection.Event.Subscribe(_ => observed_events.Add(_));
        }

        void Because()
        {
            projection.OnNext(
                new Event(
                    0,
                    event_a,
                    DateTimeOffset.UtcNow,
                    "30c1ebf5-cc30-4216-afed-e3e0aefa1316",
                    new()), new());

            projection.OnNext(
                new Event(
                    0,
                    event_b,
                    DateTimeOffset.UtcNow,
                    "30c1ebf5-cc30-4216-afed-e3e0aefa1316",
                    new()), initial_state);
        }

        [Fact] void should_only_observe_one_event() => observed_events.Count.ShouldEqual(1);
        [Fact] void should_observe_the_event_of_interest() => observed_events[0].Event.Type.ShouldEqual(event_b);
        [Fact] void should_pass_along_the_initial_state() => observed_events[0].Changeset.InitialState.ShouldEqual(initial_state);
    }
}