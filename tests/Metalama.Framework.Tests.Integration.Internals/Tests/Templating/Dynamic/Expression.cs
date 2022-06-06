using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Expression
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.DefineExpression( meta.Target.Method.Name, out var exp1 );
            var x = exp1.Value;
            meta.DefineExpression( meta.Target.Parameters[0].Value, out IExpression exp2 );
            exp2.Value = 5;
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}