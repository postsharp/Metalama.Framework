using System.Collections;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.RunTimeIsPatternVariable
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if ( meta.Target.Parameters[0].Value is IEnumerable a )
            {
                a.GetEnumerator();
            }
            

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