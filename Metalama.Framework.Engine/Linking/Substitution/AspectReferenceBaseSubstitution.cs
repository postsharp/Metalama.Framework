// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution;

/// <summary>
/// Substitutes an aspect reference that points to the base declaration (override or hidden slot).
/// </summary>
internal class AspectReferenceBaseSubstitution : AspectReferenceRenamingSubstitution
{
    public AspectReferenceBaseSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext, aspectReference )
    {
        // Support only base semantics.
        Invariant.Assert( aspectReference.ResolvedSemantic.Kind == IntermediateSymbolSemanticKind.Base );
        Invariant.Assert( aspectReference.ResolvedSemantic.Symbol.IsOverride || aspectReference.ResolvedSemantic.Symbol.TryGetHiddenSymbol( this.CompilationContext.Compilation, out _ ) );

        // Auto properties and event field default semantics should not get here.
        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IPropertySymbol property }
            && property.IsAutoProperty() == true );

        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IEventSymbol @event }
            && @event.IsEventField() == true );
    }

    public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

        if ( this.AspectReference.RootNode != this.AspectReference.SymbolSourceNode )
        {
            // Root node is different that symbol source node - this is introduction in form:
            // <helper_type>.<helper_member>(<symbol_source_node>);
            // We need to get to symbol source node.

            currentNode = this.AspectReference.RootNode switch
            {
                InvocationExpressionSyntax { ArgumentList: { Arguments.Count: 1 } argumentList } =>
                    argumentList.Arguments[0].Expression,
                _ => throw new AssertionFailedException( $"{this.AspectReference.RootNode.Kind()} is not in a supported form." )
            };
        }

        switch ( currentNode )
        {
            case MemberAccessExpressionSyntax memberAccessExpression:
                if ( targetSymbol.IsStatic )
                {
                    if ( !targetSymbol.TryGetHiddenSymbol( this.CompilationContext.Compilation, out var hiddenSymbol ) )
                    {
                        throw new AssertionFailedException( $"Expected hidden symbol for {targetSymbol}." );
                    }

                    return memberAccessExpression
                        .WithExpression(
                            substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( hiddenSymbol.ContainingType )
                            .WithLeadingTrivia( memberAccessExpression.Expression.GetLeadingTrivia() )
                            .WithTrailingTrivia( memberAccessExpression.Expression.GetTrailingTrivia() ) );
                }
                else
                {
                    return memberAccessExpression
                        .WithExpression(
                            BaseExpression()
                            .WithLeadingTrivia( memberAccessExpression.Expression.GetLeadingTrivia() )
                            .WithTrailingTrivia( memberAccessExpression.Expression.GetTrailingTrivia() ) );
                }

            default:
                throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
        }
    }
}