using Metalama.Framework.Code;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.RunTimeSwitchExpressionMismatch
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {    
            object o = new ();
            
            var y = o switch 
            {
                IParameter p => 1,
                _ => 0
            };
            
            return meta.Proceed();
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