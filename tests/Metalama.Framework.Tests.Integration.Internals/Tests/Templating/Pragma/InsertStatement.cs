using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.InsertStatement
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            // Passing a string.
            meta.InsertStatement( "for (;;) {}" );

            // Passing an IStatement.
            var statement = StatementFactory.Parse( "return 5;" );
            meta.InsertStatement( statement );

            // Padding an IExpression.
            meta.InsertStatement( ExpressionFactory.Parse( "a.ToString()" ) );

            return default;
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}