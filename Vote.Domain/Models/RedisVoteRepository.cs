using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Vote.Domain.Models
{
    public class RedisVoteRepository : IVoteRepository
    {
        public RedisVoteRepository(IConnectionMultiplexer connection, ILogger<RedisVoteRepository> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private const string VOTE_OPTION_KEY_FORMAT = "voterepo/voteoptions/{0}/{1}";
        private const string VOTE_KEY_FORMAT = "voterepo/elections/{0}";

        private readonly IConnectionMultiplexer _connection;
        private readonly ILogger<RedisVoteRepository> _logger;

        private static string GetVoteOptionKey(string electionId, string voteOptionId)
            => String.Format(VOTE_OPTION_KEY_FORMAT, electionId, voteOptionId);
        private static string GetElectionKey(string electionId)
            => String.Format(VOTE_KEY_FORMAT, electionId);

        public async Task<VoteOption> RaiseVoteCount(string electionId, string voteOptionId)
        {
            var db = _connection.GetDatabase();
            var str = db.StringGet(GetVoteOptionKey(electionId, voteOptionId)).ToString();
            var voteOption = JsonConvert.DeserializeObject<VoteOption>(str);
            voteOption.Count++;
            str = JsonConvert.SerializeObject(voteOption);
            await _connection.GetDatabase().StringSetAsync(GetVoteOptionKey(electionId, voteOptionId), str);
            return voteOption;
        }

        public async Task<Election> GetElection(string electionId)
        {
            string str = await _connection.GetDatabase().StringGetAsync(GetElectionKey(electionId));
            return JsonConvert.DeserializeObject<Election>(str);
        }

        public async Task<IEnumerable<Election>> GetElections()
        {
            var elections = new ConcurrentBag<Election>();
            var keys = _connection.GetServer(_connection.GetEndPoints().First()).Keys(pattern: GetElectionKey("*")).ToList();
            var db = _connection.GetDatabase();
            Parallel.ForEach(keys, key =>
            {
                var str = db.StringGet(key.ToString()).ToString();
                var obj = JsonConvert.DeserializeObject<Election>(str);
                elections.Add(obj);
            });
            return elections;
        }

        public async Task<Election> AddElection(string electionName)
        {
            var election = new Election()
            {
                Id = Guid.NewGuid().ToString(),
                Title = electionName
            };
            string serialized = JsonConvert.SerializeObject(election);
            await _connection.GetDatabase().StringSetAsync(GetElectionKey(election.Id), serialized);
            return election;
        }

        public async Task<VoteOption> AddVoteOption(string electionId, string name)
        {
            var voteOption = new VoteOption()
            {
                Count = 0,
                Title = name,
                ElectionId = electionId,
                Id = Guid.NewGuid().ToString()
            };
            string serialized = JsonConvert.SerializeObject(voteOption);
            await _connection.GetDatabase().StringSetAsync(GetVoteOptionKey(electionId, voteOption.Id), serialized);
            return voteOption;
        }

        public async Task<IEnumerable<VoteOption>> GetVoteOptions(string electionId)
        {
            var voteOptions = new ConcurrentBag<VoteOption>();
            var keys = _connection.GetServer(_connection.GetEndPoints().First()).Keys(pattern: GetVoteOptionKey(electionId, "*")).ToList();
            var db = _connection.GetDatabase();
            Parallel.ForEach(keys, key =>
            {
                var str = db.StringGet(key.ToString()).ToString();
                var obj = JsonConvert.DeserializeObject<VoteOption>(str);
                voteOptions.Add(obj);
            });
            return voteOptions;
        }
    }
}
