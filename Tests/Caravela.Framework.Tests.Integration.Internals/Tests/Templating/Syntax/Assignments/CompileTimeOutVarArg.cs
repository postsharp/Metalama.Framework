using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeOutVarArg
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var d = meta.CompileTime(new Dictionary<int,int>());
            d.Add(0, 5);
            d.TryGetValue( 0, out var x );
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