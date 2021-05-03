using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.SimpleCompileTimeAssignment
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = compileTime(0);
            x = 0;
            x += 4;
            x *= 2;
            x /= 2;
            x -= 2;
            x |= 128;
            x &= 127;
            
            var y = compileTime<StringBuilder>(null);
            y ??= new StringBuilder();
            y.Append("yy");
           
            
            
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