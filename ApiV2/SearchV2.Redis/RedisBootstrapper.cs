using MoreLinq;
using SearchV2.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.Redis
{
    public class RedisBootstrapper
    {
        public static async Task Load<TDoc>(string connectionString, int database, IEnumerable<TDoc> items, ISerializer<TDoc> serializer, bool drop = false) where TDoc : IWithReference<string>
        {
            var db = ConnectionMultiplexer.Connect(connectionString).GetDatabase(database);
            var batches = items.Select(i => new KeyValuePair<RedisKey, RedisValue>(i.Ref, serializer.Serialize(i))).Batch(1000);
            foreach (var batch in batches)
            {
                await db.StringSetAsync(batch.ToArray());
            }
        }
    }
}
