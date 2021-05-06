using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.InRuntimeConditional
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            if ( DateTime.Today.Day == 1 )
            {
                x ++;
                x --;
                ++ x;
                -- x;
            }
            
            
            
            meta.Comment( "x = " + x.ToString() );
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