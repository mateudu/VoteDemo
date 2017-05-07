using System;
using System.Linq;
using StackExchange.Redis;
using Vote.Domain.Application.Commands;
using Vote.Domain.Application.Handlers;
using Vote.Domain.Models;

namespace Vote.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new ConfigurationOptions();
            options.EndPoints.Add("localhost:6379");
            var connection = ConnectionMultiplexer.Connect(options);
            var repo = new RedisVoteRepository(connection);

            //for (int i = 0; i < 3; i++)
            //{
            //    repo.AddElection("electiontest" + (10 * (i + 1))).Wait();
            //}
            var elections = repo.GetElections().Result;
            foreach (var el in elections)
            {
                var opts = repo.GetVoteOptions(el.Id).Result;
                repo.RaiseVoteCount(el.Id, opts.First().Id).Wait();
                Console.WriteLine(el.Title + " " + repo.GetVoteOptions(el.Id).Result.Count());
            }
            //var handler = new VoteMadeEventHandler(repo);
            //handler.Handle(new VoteMadeEvent("election1", "134"));
            Console.WriteLine("Hello World!");
        }
    }
}