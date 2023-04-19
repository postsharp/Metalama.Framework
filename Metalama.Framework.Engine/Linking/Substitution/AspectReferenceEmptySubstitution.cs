// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution;

/// <summary>
/// Substitutes an aspect reference that points to the base declaration (override or a hidden slot).
/// </summary>
internal class AspectReferenceEmptySubstitution : AspectReferenceRenamingSubstitution
{
    public AspectReferenceEmptySubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext, aspectReference )
    {
        // Support only base semantics.
        Invariant.Assert( aspectReference.ResolvedSemantic.Kind == IntermediateSymbolSemanticKind.Base );
        Invariant.Assert( !aspectReference.ResolvedSemantic.Symbol.IsOverride && !aspectReference.ResolvedSemantic.Symbol.TryGetHiddenSymbol( this.CompilationContext.Compilation, out _ ) );

        // Auto properties and event field default semantics should not get here.
        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IPropertySymbol property }
            && property.IsAutoProperty() == true );

        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IEventSymbol @event }
            && @event.IsEventField() == true );
    }

    public override string GetTargetMemberName()
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;
        return LinkerRewritingDriver.GetEmptyImplMemberName( targetSymbol );
    }

    public override SyntaxNode? SubstituteMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
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
            .WithName( this.RewriteName( currentNode.Name, this.GetTargetMemberName() ) );
    }
}