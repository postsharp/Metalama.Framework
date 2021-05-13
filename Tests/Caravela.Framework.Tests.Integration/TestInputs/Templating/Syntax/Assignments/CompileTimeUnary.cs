using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeUnary
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            x ++;
            x --;
            ++ x;
            -- x;
            
            
            
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