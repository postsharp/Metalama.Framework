using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeUnary
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = compileTime(0);
            x ++;
            x --;
            ++ x;
            -- x;
            
            
            
            pragma.Comment( "x = " + x.ToString() );
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