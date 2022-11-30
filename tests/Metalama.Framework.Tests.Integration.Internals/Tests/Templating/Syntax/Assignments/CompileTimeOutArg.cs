using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.CompileTimeOutArg
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            var d = meta.CompileTime(new Dictionary<int,int>());
            d.Add(0, 5);
            d.TryGetValue( 0, out x );
            meta.InsertComment("x = "+x);

            return null;
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}