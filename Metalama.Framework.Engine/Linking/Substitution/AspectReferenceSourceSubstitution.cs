// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution;

/// <summary>
/// Substitutes an aspect reference that points to the default semantic that points to the original source.
/// </summary>
internal class AspectReferenceSourceSubstitution : AspectReferenceRenamingSubstitution
{
    public AspectReferenceSourceSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext, aspectReference )
    {
        // Support only base semantics.
        Invariant.Assert(
            aspectReference.ResolvedSemantic.Kind == IntermediateSymbolSemanticKind.Default
            || aspectReference.ResolvedSemantic is
                { Symbol: { IsOverride: true, IsSealed: false } or { IsVirtual: true }, Kind: IntermediateSymbolSemanticKind.Base } );

        // Auto properties and event field default semantics should not get here.
        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IPropertySymbol property }
            && property.IsAutoProperty() == true );

        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IEventSymbol @event }
            && @event.IsEventField() == true );
    }

    protected override string GetTargetMemberName()
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

        return LinkerRewritingDriver.GetOriginalImplMemberName( targetSymbol );
    }

    protected override SyntaxNode SubstituteMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

        ExpressionSyntax expression =
            targetSymbol.IsStatic
                ? substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType )
                : ThisExpression();

        return currentNode
            .WithExpression(
                expression
                    .WithLeadingTrivia( currentNode.Expression.GetLeadingTrivia() )
                    .WithTrailingTrivia( currentNode.Expression.GetTrailingTrivia() ) )
            .WithName( RewriteName( currentNode.Name, this.GetTargetMemberName() ) );
    }
}