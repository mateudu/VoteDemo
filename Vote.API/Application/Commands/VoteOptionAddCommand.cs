using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vote.API.Application.Commands
{
    public class VoteOptionAddCommand
    {
        public string ElectionId { get; set; }
        public string Title { get; set; }
    }
}
