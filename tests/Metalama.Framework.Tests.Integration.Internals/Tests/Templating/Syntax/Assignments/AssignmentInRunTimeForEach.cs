using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.AssignmentInRunTimeForEach
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            
            foreach ( var i in Enumerable.Range(0,3))
            {
                x = x + 1;
            }
            
            
            
            meta.InsertComment( "x = " + x.ToString());
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