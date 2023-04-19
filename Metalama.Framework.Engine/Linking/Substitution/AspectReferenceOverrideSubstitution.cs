// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes non-inlined aspect reference.
    /// </summary>
    internal sealed partial class AspectReferenceOverrideSubstitution : AspectReferenceRenamingSubstitution
    {
        public AspectReferenceOverrideSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext, aspectReference )
        {
            Invariant.Assert( aspectReference.ResolvedSemantic.Kind is IntermediateSymbolSemanticKind.Default or IntermediateSymbolSemanticKind.Final );

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
            return targetSymbol.Name;
        }

        public override SyntaxNode? SubstituteMemberAccess( MemberAccessExpressionSyntax currentNode, SubstitutionContext substitutionContext )
        {
            var targetSymbol = this.AspectReference.ResolvedSemantic.Symbol;

            if ( SymbolEqualityComparer.Default.Equals(
                    this.AspectReference.ContainingSemantic.Symbol.ContainingType,
                    targetSymbol.ContainingType ) )
            {
                if ( this.AspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                {
                    return currentNode
                        .WithExpression( ThisExpression() )
                        .WithName( this.RewriteName( currentNode.Name, this.GetTargetMemberName() ) );
                }
                else
                {
                    return currentNode
                        .WithName( this.RewriteName( currentNode.Name, this.GetTargetMemberName() ) );
                }
            }
            else
            {
                if ( targetSymbol.IsStatic )
                {
                    return
                        MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                substitutionContext.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ),
                                this.RewriteName( currentNode.Name, this.GetTargetMemberName() ) )
                            .WithLeadingTrivia( currentNode.GetLeadingTrivia() )
                            .WithTrailingTrivia( currentNode.GetTrailingTrivia() );
                }
                else
                {

                    if ( this.CompilationContext.SymbolComparer.Is(
                            targetSymbol.ContainingType,
                            this.AspectReference.ContainingSemantic.Symbol.ContainingType ) )
                    {
                        throw new AssertionFailedException( "Resolved symbol is declared in a derived class." );
                    }
                    else if ( this.CompilationContext.SymbolComparer.Is(
                                 this.AspectReference.ContainingSemantic.Symbol.ContainingType,
                                 targetSymbol.ContainingType ) )
                    {
                        // Leave as-is because this is a reference to a declaration that is not overridden in this class.
                        return currentNode;
                    }
                    else
                    {
                        // Resolved symbol is unrelated to the containing symbol.
                        return currentNode
                            .WithName( this.RewriteName(currentNode.Name, this.GetTargetMemberName()) );
                    }
                }
            }
        }

        private IAspectInstanceInternal ResolveAspectInstance( SubstitutionContext context )
        {
            var injectedMember = context.RewritingDriver.InjectionRegistry.GetInjectedMemberForSymbol( this.AspectReference.ContainingSemantic.Symbol );

            return injectedMember.AssertNotNull().Transformation.ParentAdvice.Aspect;
        }
    }
}