// Warning CS0162 on `return`: `Unreachable code detected`

using System.Collections.Concurrent;

public class SelfCachedClass
{
    [Cache]
    public int Add( int a, int b )
    {
        {
            var cacheKey = $"Add({string.Join( ", ", new object[] { a, b } )})";

            if (!_cache.TryGetValue( cacheKey, out var returnValue ))
            {
                returnValue = a + b;
                _cache.TryAdd( cacheKey, returnValue );
            }

            return (int)returnValue;
        }

        return default;
    }

    [CacheAndRetry]
    public int Rmove( int a, int b )
    {
        {
            var cacheKey = $"Rmove({string.Join( ", ", new object[] { a, b } )})";

            if (!_cache.TryGetValue( cacheKey, out var returnValue ))
            {
                returnValue = a - b;
                _cache.TryAdd( cacheKey, returnValue );
            }

            return (int)returnValue;
        }

        return default;
    }

    private ConcurrentDictionary<string, object> _cache = (ConcurrentDictionary<string, object>)( new ConcurrentDictionary<string, object>() );
}