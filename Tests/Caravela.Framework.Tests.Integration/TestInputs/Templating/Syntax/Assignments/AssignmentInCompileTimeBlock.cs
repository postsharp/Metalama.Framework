using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.AssignmentInCompileTimeBlock
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            
            foreach ( var p in meta.Parameters )
            {
                x++;
            }
            
            
            
            meta.Comment( "x = " + x.ToString());
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