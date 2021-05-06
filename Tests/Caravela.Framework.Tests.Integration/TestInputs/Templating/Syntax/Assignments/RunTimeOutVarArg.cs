using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.RunTimeOutVarArg
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var d = new Dictionary<int,int>();
            d.Add(0, 5);
            d.TryGetValue( 0, out var x );

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