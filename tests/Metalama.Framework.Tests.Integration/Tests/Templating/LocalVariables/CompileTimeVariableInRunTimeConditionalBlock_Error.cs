using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.CompileTimeVariableInRunTimeConditionalBlock_Error;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        int a = 0;
        int i = meta.CompileTime(0);

        a++;
        i++;

        if (meta.Target.Parameters[0].Value > 0)
        {
            int b = 0;
            int j = meta.CompileTime(0);

            a++;
            b++;
            i++;
            j++;

            if (meta.Target.Parameters[1].Value > 0)
            {
                int c = 0;
                int k = meta.CompileTime(0);

                a++;
                b++;
                c++;
                i++;
                j++;
                k++;
            }
        }

        return meta.Proceed();
    }
}

class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}