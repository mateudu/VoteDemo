using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vote.API
{
    public class VoteAPISettings
    {
        public string ConnectionString { get; set; }
        public string RedisHost { get; set; }
        public string RedisPort { get; set; }
        public string EventBusConnection { get; set; }
    }
}
