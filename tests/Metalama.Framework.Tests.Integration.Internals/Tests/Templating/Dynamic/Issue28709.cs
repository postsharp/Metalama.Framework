using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.Framework;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Issue28709
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
             foreach (var p in meta.Target.Parameters)
            {
                Console.WriteLine( "IsOut=" + (p.RefKind == RefKind.Out));
            }
            
            return default;
        }
    }
    
    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    // <target>
    class TargetCode
    {
        static int Method(int a, int b, out int c )
        {
            Console.WriteLine("Thinking...");
            return c = a + b;
        }
    }

}
