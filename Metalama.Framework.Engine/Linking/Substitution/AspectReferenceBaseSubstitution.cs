﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution;

/// <summary>
/// Substitutes an aspect reference that points to the base declaration (override or hidden slot).
/// </summary>
internal sealed class AspectReferenceBaseSubstitution : AspectReferenceRenamingSubstitution
{
    public AspectReferenceBaseSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base(
        compilationContext,
        aspectReference )
    {
        // Support only base semantics.
        Invariant.Assert( aspectReference.ResolvedSemantic.Kind == IntermediateSymbolSemanticKind.Base );

        Invariant.Assert(
            aspectReference.ResolvedSemantic.Symbol.IsOverride
            || aspectReference.ResolvedSemantic.Symbol.TryGetHiddenSymbol( this.CompilationContext.Compilation, out _ )
            || !compilationContext.SymbolComparer.Equals(
                aspectReference.ContainingSemantic.Symbol.ContainingType,
                aspectReference.ResolvedSemantic.Symbol.ContainingType ) );

        // Auto properties and event field default semantics should not get here.
        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IPropertySymbol property }
            && property.IsAutoProperty() == true );

        Invariant.AssertNot(
            aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IEventSymbol @event }
            && @event.IsEventField() == true );

        if ( aspectReference is { HasCustomReceiver: true, ResolvedSemantic.Symbol.IsStatic: false } )
        {
            throw AspectLinkerDiagnosticDescriptors.CannotInvokeAnotherInstanceBaseRequired.CreateException( aspectReference.OriginalSymbol );
        }
    }

    protected override string GetTargetMemberName()
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

        return targetSymbol.Name;
    }

    protected override SyntaxNode SubstituteFinalizerMemberAccess( MemberAccessExpressionSyntax currentNode )
    {
        return
            IdentifierName( "__LINKER_TO_BE_REMOVED__" )
                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.NullAspectReferenceExpression )
                .WithLeadingTrivia( currentNode.GetLeadingTrivia() )
                .WithTrailingTrivia( currentNode.GetTrailingTrivia() );
    }

    protected override SyntaxNode SubstituteMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
    {
        var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

        if ( targetSymbol.IsStatic )
        {
            if ( !targetSymbol.TryGetHiddenSymbol( this.CompilationContext.Compilation, out var hiddenSymbol ) )
            {
                throw new AssertionFailedException( $"Expected hidden symbol for {targetSymbol}." );
            }

            return currentNode
                .WithExpression(
                    substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( hiddenSymbol.ContainingType )
                        .WithLeadingTrivia( currentNode.Expression.GetLeadingTrivia() )
                        .WithTrailingTrivia( currentNode.Expression.GetTrailingTrivia() ) );
        }
        else
        {
            return currentNode
                .WithExpression(
                    BaseExpression()
                        .WithLeadingTrivia( currentNode.Expression.GetLeadingTrivia() )
                        .WithTrailingTrivia( currentNode.Expression.GetTrailingTrivia() ) );
        }
    }

    protected override SyntaxNode SubstituteElementAccess( ElementAccessExpressionSyntax currentNode )
    {
        return currentNode
            .WithExpression(
                BaseExpression()
                    .WithLeadingTrivia( currentNode.Expression.GetLeadingTrivia() )
                    .WithTrailingTrivia( currentNode.Expression.GetTrailingTrivia() ) );
    }
}