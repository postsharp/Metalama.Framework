using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Expression
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            ExpressionFactory.Capture( meta.Target.Method.Name, out var exp1 );
            var x = exp1.Value;
            ExpressionFactory.Capture( meta.Target.Parameters[0].Value, out IExpression exp2 );
            exp2.Value = 5;

            return default;
        }
    }

    // <target>
    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}