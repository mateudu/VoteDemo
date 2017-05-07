using VoteDemo.BuildingBlocks.EventBus.Events;

namespace Vote.API.Application.Commands
{
    public class MakeVoteCommand
    {
        public string ElectionId { get; set; }
        public string VoteOptionId { get; set; }
    }
}
