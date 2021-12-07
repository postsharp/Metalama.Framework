using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.InsertStatement
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Passing a string.
            meta.InsertStatement("for (;;) {}");
            
            // Passing an IStatement.
            var statement = meta.ParseStatement("return 5;");
            meta.InsertStatement( statement );
            
            // Padding an IExpression.
            meta.InsertStatement( meta.ParseExpression( "a.ToString()" ) );
            return default;
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