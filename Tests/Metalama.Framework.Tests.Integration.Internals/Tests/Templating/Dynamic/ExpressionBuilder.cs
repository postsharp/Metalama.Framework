using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicExpressionBuilder
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var expressionBuilder = new ExpressionBuilder();
            expressionBuilder.AppendVerbatim("Test( ");
            expressionBuilder.AppendLiteral(1);
            expressionBuilder.AppendVerbatim(", ");
            expressionBuilder.AppendLiteral(1D);
            expressionBuilder.AppendVerbatim(", ");
            expressionBuilder.AppendLiteral(1D, true);
            expressionBuilder.AppendVerbatim(", ");
            expressionBuilder.AppendLiteral(1F);
            expressionBuilder.AppendVerbatim(", ");
            expressionBuilder.AppendLiteral("s\"\n");
            expressionBuilder.AppendVerbatim(", ");

            var arrayBuilder = new ArrayBuilder();
            arrayBuilder.Add( 1 );
            arrayBuilder.Add( 2 );
            arrayBuilder.Add( 3 );
            
            
            expressionBuilder.AppendExpression( arrayBuilder );
            expressionBuilder.AppendVerbatim(", ");
            expressionBuilder.AppendExpression( meta.Target.Parameters[0] );
            expressionBuilder.AppendVerbatim(", typeof(");
            expressionBuilder.AppendTypeName( ( (IExpression)meta.Target.Parameters[0] ).Type );
            expressionBuilder.AppendVerbatim(") )");
            
            
            var expression = expressionBuilder.ToExpression();
            Console.WriteLine($"type={expression.Type}");
            var value = expressionBuilder.ToValue();

            return default;
        }
    }

    // <target>
    internal class TargetCode
    {
        private int Method( int a, string c, DateTime dt )
        {
            return a;
        }
    }
}