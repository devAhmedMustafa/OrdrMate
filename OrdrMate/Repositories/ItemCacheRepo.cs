using OrdrMate.Models;
using StackExchange.Redis;

namespace OrdrMate.Repositories;

public class CategoryItemsCacheRepo(IConnectionMultiplexer redis) : CacheRepo<IEnumerable<Item>>(redis)
{
}