using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.CompileTimeAssignmentExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            var y = meta.CompileTime(0);
            
            x = y = 1;
            
            
            meta.InsertComment( "x = " + x.ToString(), "y = " + y.ToString());
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