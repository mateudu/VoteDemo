using System;
using System.Collections.Generic;
using System.Text;
using VoteDemo.BuildingBlocks.EventBus.Events;

namespace Vote.Domain.Application.Events
{
    public class ElectionMadeEvent : IntegrationEvent
    {
        public string Title { get; private set; }

        public ElectionMadeEvent(string title)
        {
            Title = title;
        }
    }
}
