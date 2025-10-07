using OrdrMate.Models;
using StackExchange.Redis;

namespace OrdrMate.Repositories;

public class ItemCacheRepo(IConnectionMultiplexer redis) : CacheRepo<List<Item>>(redis)
{
}