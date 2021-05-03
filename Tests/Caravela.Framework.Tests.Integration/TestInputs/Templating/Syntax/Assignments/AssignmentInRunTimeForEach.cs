using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.AssignmentInRunTimeForEach
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = compileTime(0);
            
            foreach ( var i in Enumerable.Range(0,3))
            {
                x = x + 1;
            }
            
            
            
            pragma.Comment( "x = " + x.ToString());
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