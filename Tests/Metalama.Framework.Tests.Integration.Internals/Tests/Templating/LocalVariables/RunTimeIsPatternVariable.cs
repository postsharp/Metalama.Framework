using System.Collections;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

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