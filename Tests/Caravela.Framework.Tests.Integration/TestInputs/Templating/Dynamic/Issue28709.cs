using System;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.Issue28709
{
      [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
             foreach (var p in meta.Parameters)
            {
                Console.WriteLine( "IsOut=" + p.IsOut());
            }
            
            return default;
        }
    }
    
    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    [TestOutput]
    class TargetCode
    {
        static int Method(int a, int b, out int c )
        {
            Console.WriteLine("Thinking...");
            return c = a + b;
        }
    }

}
