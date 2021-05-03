using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeOutVarArg
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var d = compileTime(new Dictionary<int,int>());
            d.Add(0, 5);
            d.TryGetValue( 0, out var x );
            pragma.Comment("x = "+x);

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