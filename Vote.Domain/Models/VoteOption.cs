using System;
using System.Collections.Generic;
using System.Text;

namespace Vote.Domain.Models
{
    public class VoteOption
    {
        public string Id { get; set; }
        public string ElectionId { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
    }
}
