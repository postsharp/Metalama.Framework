using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseTemplateOnlyInCompileTimeOnlyNonTemplate;

[CompileTime]
internal class C
{
    private void M()
    {
        meta.Proceed();

        meta.ProceedAsync();

        meta.InsertStatement(ExpressionFactory.Capture(42));
    }
}