using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.LambdaParameter_CompileTime;

internal class Aspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceMethod(builder.Target.DeclaringType, nameof(PropertyBody), args: new { propertyBody = GetPropertyBody(builder.Target) });
    }

    [CompileTime]
    string? GetPropertyBody(IProperty property)
    {
        var methodBody = property.GetSymbol()
            ?.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .Cast<PropertyDeclarationSyntax>()
            .Select(SyntaxNode? (p) =>
            {
                if (p.ExpressionBody != null)
                {
                    return p.ExpressionBody;
                }

                var getter = p.AccessorList?.Accessors
                    .SingleOrDefault(a => a.Keyword.IsKind(SyntaxKind.GetKeyword));

                return (SyntaxNode?)getter?.ExpressionBody ?? getter?.Body;
            })
            .WhereNotNull()
            .FirstOrDefault();

        return methodBody?.ToString();
    }

    [Template]
    string? PropertyBody([CompileTime] string propertyBody) => propertyBody;
}

// <target>
internal class TargetCode
{
    [Aspect]
    public int P { get => 42; }
}