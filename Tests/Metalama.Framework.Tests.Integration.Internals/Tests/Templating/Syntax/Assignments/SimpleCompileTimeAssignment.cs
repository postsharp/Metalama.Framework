using System.Text;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.SimpleCompileTimeAssignment
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            x = 0;
            x += 4;
            x *= 2;
            x /= 2;
            x -= 2;
            x |= 128;
            x &= 127;
            
            var y = meta.CompileTime<StringBuilder>(null);
            y ??= new StringBuilder();
            y.Append("yy");
           
            
            
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