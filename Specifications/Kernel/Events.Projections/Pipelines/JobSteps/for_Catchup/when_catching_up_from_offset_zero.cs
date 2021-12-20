// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Events.Projections.Pipelines.JobSteps.for_Catchup
{
    public class when_catching_up_from_offset_zero : given.no_events
    {
        void Establish() => positions.Setup(_ => _.GetFor(projection.Object, configuration)).Returns(Task.FromResult<EventLogSequenceNumber>(0));

        async Task Because() => await catchup.Perform(job_status);

        [Fact] void should_prepare_initial_run_on_result_store() => result_store.Verify(_ => _.PrepareInitialRun(), Once());
    }
}