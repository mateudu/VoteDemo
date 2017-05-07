using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Semantics;
using Vote.API.Application.Commands;
using Vote.Domain.Application.Events;
using Vote.Domain.Models;
using VoteDemo.BuildingBlocks.EventBus.Abstractions;

namespace Vote.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Elections")]
    public class ElectionsController : Controller
    {
        public ElectionsController(IEventBus eventBus, IVoteRepository respository)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _repository = respository ?? throw new ArgumentNullException(nameof(respository));
        }

        private readonly IEventBus _eventBus;
        private readonly IVoteRepository _repository;

        // GET: api/Elections
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Election>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get()
        {
            var elections = await _repository.GetElections();
            return StatusCode((int) HttpStatusCode.OK, elections);
        }

        // GET: api/Elections/5
        [HttpGet("{id}", Name = "Get")]
        [ProducesResponseType(typeof(Election), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            var election = await _repository.GetElection(id);
            return StatusCode((int)HttpStatusCode.OK, election);
        }
        
        // POST: api/Elections
        [HttpPost]
        public IActionResult Post([FromBody]ElectionMakeCommand command)
        {
            var @event = new ElectionMadeEvent(command.Title);
            _eventBus.Publish(@event);
            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}
