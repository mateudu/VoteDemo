using System;
using System.Collections.Generic;
using System.Text;
using VoteDemo.BuildingBlocks.EventBus.Events;

namespace Vote.Domain.Application.Events
{
    public class VoteOptionAddedEvent : IntegrationEvent
    {
        public string ElectionId { get; private set; }
        public string Title { get; private set; }

        public VoteOptionAddedEvent(string electionId, string title)
        {
            ElectionId = electionId;
            Title = title;
        }
    }
}
