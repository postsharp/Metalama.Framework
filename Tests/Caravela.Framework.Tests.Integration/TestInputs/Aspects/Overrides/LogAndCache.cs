using System;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache;
using Caravela.TestFramework;


[assembly: AspectOrderAttribute(typeof(LogAttribute), typeof(CacheAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache
{

    [TestOutput]
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
        public override dynamic OverrideMethod()
        {
            Console.WriteLine(meta.Method.ToDisplayString() + " started.");

            try
            {
                dynamic result = meta.Proceed();

                Console.WriteLine(meta.Method.ToDisplayString() + " succeeded.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(meta.Method.ToDisplayString() + " failed: " + e.Message);

                throw;
            }
        }
    }

    public class CacheAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            // Builds the caching string.
            var stringBuilder = meta.CompileTime(new StringBuilder());
            stringBuilder.Append(meta.Type.ToString());
            stringBuilder.Append('.');
            stringBuilder.Append(meta.Method.Name);
            stringBuilder.Append('(');
            int i = meta.CompileTime(0);
            foreach (var p in meta.Parameters)
            {
                string comma = i > 0 ? ", " : "";

                if (p.IsOut())
                {
                    stringBuilder.Append($"{comma}{p.Name} = <out> ");
                }
                else
                {
                    stringBuilder.Append($"{comma}{{{i}}}");
                }

                i++;
            }

            stringBuilder.Append(')');

            string cacheKey = string.Format(stringBuilder.ToString(), meta.Parameters.Values.ToArray());

            // Cache lookup.
            if (SampleCache.Cache.TryGetValue(cacheKey, out object? value))
            {
                Console.WriteLine("Cache hit.");
                return value;
            }
            else
            {
                Console.WriteLine("Cache miss.");
                dynamic result = meta.Proceed();

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