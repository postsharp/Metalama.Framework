using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.Expression
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