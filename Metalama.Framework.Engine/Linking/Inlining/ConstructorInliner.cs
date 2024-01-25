// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Linking.Inlining;

/// <summary>
/// Constructor inliner.
/// </summary>
internal sealed class ConstructorInliner : Inliner
{
    private static bool IsInlineableObjectCreation(
        SemanticModel semanticModel,
        IMethodSymbol contextConstructor,
        ObjectCreationExpressionSyntax objectCreationExpression )
    {
        var expectedNumberOfParameters =
            contextConstructor.Parameters.LastOrDefault()?.Name == AspectReferenceSyntaxProvider.LinkerOverrideParamName
            ? contextConstructor.Parameters.Length - 1
            : contextConstructor.Parameters.Length;

        return
            expectedNumberOfParameters == (objectCreationExpression.ArgumentList?.Arguments.Count ?? 0)
            && (objectCreationExpression.ArgumentList?.Arguments
               .Select( ( x, i ) => (Argument: x.Expression, Index: i) )
               ?.All( a => SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo( a.Argument ).Symbol, contextConstructor.Parameters[a.Index] ) )
              ?? false);
    }

    public override bool IsValidForTargetSymbol( ISymbol symbol )
        => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor };

    public override bool IsValidForContainingSymbol( ISymbol symbol ) => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor };

    public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
    {
        if ( !base.CanInline( aspectReference, semanticModel ) )
        {
            return false;
        }

        // The syntax has to be in form: <annotated_constructor_expression>( new <type>(<arguments>) );
        if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } )
        {
            // Coverage: ignore (hit only when the check in base class is incorrect).
            return false;
        }

        if ( aspectReference.RootExpression is not InvocationExpressionSyntax invocationExpression )
        {
            return false;
        }

        if ( invocationExpression.Parent is not ExpressionStatementSyntax )
        {
            return false;
        }

        if ( invocationExpression.ArgumentList is not { Arguments: [{ Expression: ObjectCreationExpressionSyntax { } objectCreationExpression }] } )
        {
            return false;
        }

        // The invocation needs to be inlineable in itself.
        if ( !IsInlineableObjectCreation( semanticModel, aspectReference.ContainingSemantic.Symbol, objectCreationExpression ) )
        {
            return false;
        }

        return true;
    }

    public override InliningAnalysisInfo GetInliningAnalysisInfo( ResolvedAspectReference aspectReference )
    {
        return new InliningAnalysisInfo( aspectReference.RootExpression.Parent.AssertNotNull(), null );
    }

    public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
    {
        return
            linkedTargetBody
                .WithLeadingTrivia( currentNode.GetLeadingTrivia().AddRange( linkedTargetBody.GetLeadingTrivia() ) )
                .WithTrailingTrivia( linkedTargetBody.GetTrailingTrivia().AddRange( currentNode.GetTrailingTrivia() ) );
    }
}