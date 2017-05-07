using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vote.Domain.Application.Commands;
using Vote.Domain.Models;
using VoteDemo.BuildingBlocks.EventBus.Abstractions;

namespace Vote.Domain.Application.Handlers
{
    public class VoteMadeEventHandler : IIntegrationEventHandler<VoteMadeEvent>
    {
        public VoteMadeEventHandler(IVoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        private readonly IVoteRepository _repository;
        public async Task Handle(VoteMadeEvent @event)
        {
            await _repository.RaiseVoteCount(@event.ElectionId, @event.VoteOptionId);
        }
    }
}
