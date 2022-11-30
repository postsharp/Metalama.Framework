using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.CollectionInitializer
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = new List<int> { 1, 2, 3 };
            var y = new List<string> { meta.Target.Parameters.Count.ToString(), "a", "b" };
            Console.WriteLine( string.Join( ", ", y ) );
            var z = new List<int> { meta.Target.Parameters[0].Value, 2, 3 };
            return default;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}