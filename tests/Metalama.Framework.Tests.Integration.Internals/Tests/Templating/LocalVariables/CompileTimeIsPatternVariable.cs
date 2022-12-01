using System.Collections;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.CompileTimeIsPatternVariable
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if ( meta.Target.Parameters is IEnumerable disposable )
            {
                disposable.GetEnumerator();
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