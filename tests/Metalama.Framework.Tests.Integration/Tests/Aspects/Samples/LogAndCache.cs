using System;
using System.Text;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache;

[assembly: AspectOrderAttribute(AspectOrderDirection.RunTime, typeof(LogAttribute), typeof(CacheAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache
{

    // <target>
    class TargetCode
    {
 
        [Log]
        [Cache]
        static int Add(int a, int b)
        {
            Console.WriteLine("Thinking...");
            return a + b;
        }
    }

    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(meta.Target.Method.ToDisplayString() + " started.");

            try
            {
                dynamic? result = meta.Proceed();

                Console.WriteLine(meta.Target.Method.ToDisplayString() + " succeeded.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(meta.Target.Method.ToDisplayString() + " failed: " + e.Message);

                throw;
            }
        }
    }

    public class CacheAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            // Builds the caching string.
            var stringBuilder = meta.CompileTime(new StringBuilder());
            stringBuilder.Append(meta.Target.Type.ToString());
            stringBuilder.Append('.');
            stringBuilder.Append(meta.Target.Method.Name);
            stringBuilder.Append('(');
            int i = meta.CompileTime(0);
            foreach (var p in meta.Target.Parameters)
            {
                string comma = i > 0 ? ", " : "";

                if (p.RefKind.IsReadable())
                {
                    stringBuilder.Append( $"{comma}{{{i}}}" );
                }
                else
                {
                    stringBuilder.Append( $"{comma}{p.Name} = <out> " );
                }

                i++;
            }

            stringBuilder.Append(')');

            string cacheKey = string.Format(stringBuilder.ToString(), meta.Target.Parameters.ToValueArray());

            // Cache lookup.
            if (SampleCache.Cache.TryGetValue(cacheKey, out object? value))
            {
                Console.WriteLine("Cache hit.");
                return value;
            }
            else
            {
                Console.WriteLine("Cache miss.");
                dynamic? result = meta.Proceed();

                // Add to cache.
                SampleCache.Cache.TryAdd(cacheKey, result);
                return result;
            }
        }
    }

    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.
    public static class SampleCache
    {
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> Cache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, object>();
    }
}