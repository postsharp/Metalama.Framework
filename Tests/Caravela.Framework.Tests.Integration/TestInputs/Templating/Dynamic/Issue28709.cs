using System;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.Issue28709
{

    public class CacheAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            foreach (var p in meta.Parameters)
            {
                Console.WriteLine( "IsOut=" + p.IsOut());
            }
            
            return default;
        }
    }
    
    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.
    
    class Program
    {
        [Cache]
        static int Add(int a, int b, out int c )
        {
            Console.WriteLine("Thinking...");
            return c = a + b;
        }
    }

}