using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vote.API.Application.Commands;
using Vote.Domain.Application.Commands;
using Vote.Domain.Application.Events;
using Vote.Domain.Models;
using VoteDemo.BuildingBlocks.EventBus.Abstractions;

namespace Vote.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Elections")]
    public class VotesController : Controller
    {
        public VotesController(IEventBus eventBus, IVoteRepository respository)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _repository = respository ?? throw new ArgumentNullException(nameof(respository));
        }

        private readonly IEventBus _eventBus;
        private readonly IVoteRepository _repository;

        // GET: api/Elections/123/Votes
        [HttpGet("{electionId}/VoteOptions")]
        [ProducesResponseType(typeof(IEnumerable<VoteOption>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string electionId)
        {
            var voteOptions = await _repository.GetVoteOptions(electionId);
            return StatusCode((int) HttpStatusCode.OK, voteOptions);
        }

        // GET: api/Elections/123/Votes
        [HttpPost("{electionId}/VotesOptions")]
        public IActionResult Post(string electionId, [FromBody] VoteOptionAddCommand command)
        {
            var @event = new VoteOptionAddedEvent(electionId, command.Title);
            _eventBus.Publish(@event);
            return StatusCode((int) HttpStatusCode.Created);
        }

        // GET: api/Elections/123/Votes
        [HttpPost("{electionId}/VoteOptions/{voteOptionId}/Vote")]
        public IActionResult Vote(string electionId, string voteOptionId, [FromBody] MakeVoteCommand command)
        {
            var @event = new VoteMadeEvent(electionId, command.VoteOptionId);
            _eventBus.Publish(@event);
            return StatusCode((int)HttpStatusCode.OK);
        }
    }
}
