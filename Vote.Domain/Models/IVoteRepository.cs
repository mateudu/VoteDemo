using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vote.Domain.Models
{
    public interface IVoteRepository
    {
        Task<Election> GetElection(string electionId);
        Task<IEnumerable<Election>> GetElections();
        Task<Election> AddElection(string electionName);

        Task<VoteOption> AddVoteOption(string electionId, string name);
        Task<IEnumerable<VoteOption>> GetVoteOptions(string electionId);

        Task<VoteOption> RaiseVoteCount(string electionId, string voteOptionId);
    }
}
