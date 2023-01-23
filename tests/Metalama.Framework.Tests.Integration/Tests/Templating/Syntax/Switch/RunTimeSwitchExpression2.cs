using Metalama.Framework.Code;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.RunTimeSwitchExpression2
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {    
            object o = new ();

            var y = o switch
            {
                int i => meta.Target.Parameters[0].Name.Length,
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