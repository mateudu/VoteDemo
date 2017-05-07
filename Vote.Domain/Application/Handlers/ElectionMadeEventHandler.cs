using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vote.Domain.Application.Events;
using Vote.Domain.Models;
using VoteDemo.BuildingBlocks.EventBus.Abstractions;

namespace Vote.Domain.Application.Handlers
{
    public class ElectionMadeEventHandler : IIntegrationEventHandler<ElectionMadeEvent>
    {
        public ElectionMadeEventHandler(IVoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        private readonly IVoteRepository _repository;
        
        public async Task Handle(ElectionMadeEvent @event)
        {
            await _repository.AddElection(@event.Title);
        }
    }
}
