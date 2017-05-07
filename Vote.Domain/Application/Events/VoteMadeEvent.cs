using VoteDemo.BuildingBlocks.EventBus.Events;

namespace Vote.Domain.Application.Commands
{
    public class VoteMadeEvent : IntegrationEvent
    {
        public string ElectionId { get; private set; }
        public string VoteOptionId { get; private set; }

        public VoteMadeEvent(string electionId, string voteOptionId)
        {
            ElectionId = electionId;
            VoteOptionId = voteOptionId;
        }
    }
}
