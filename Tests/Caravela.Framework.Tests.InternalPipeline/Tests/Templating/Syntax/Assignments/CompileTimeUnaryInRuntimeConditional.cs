using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

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
            
            
            
            meta.InsertComment( "x = " + x.ToString() );
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