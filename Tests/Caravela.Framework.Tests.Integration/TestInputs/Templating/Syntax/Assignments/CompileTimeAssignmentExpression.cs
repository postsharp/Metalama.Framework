using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeAssignmentExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = compileTime(0);
            var y = compileTime(0);
            
            x = y = 1;
            
            
            pragma.Comment( "x = " + x.ToString(), "y = " + y.ToString());
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