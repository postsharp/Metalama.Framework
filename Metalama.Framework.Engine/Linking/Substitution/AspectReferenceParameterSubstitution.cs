// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes non-inlined aspect reference that adds a helper parameter.
    /// </summary>
    internal sealed class AspectReferenceParameterSubstitution : SyntaxNodeSubstitution
    {
        private readonly ResolvedAspectReference _aspectReference;

        public override SyntaxNode TargetNode => this._aspectReference.RootNode;

        public AspectReferenceParameterSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base(
            compilationContext )
        {
            this._aspectReference = aspectReference;
        }

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext context )
        {
            // TODO: This not used, instead of predefined helper types, the linker should generate private helper
            //       types for each aspect layer into each target type.

            // IMPORTANT: This method needs to always strip trivia if rewriting the existing expression.
            //            Trivia existing around the expression are preserved during substitution.

            var targetSymbol = this._aspectReference.ResolvedSemantic.Symbol;
            var targetSemanticKind = this._aspectReference.ResolvedSemantic.Kind;

            var targetMemberAspectParameterType =
                targetSemanticKind switch
                {
                    IntermediateSymbolSemanticKind.Default
                        when SymbolEqualityComparer.Default.Equals(
                                 this._aspectReference.ResolvedSemantic.Symbol.ContainingType,
                                 this._aspectReference.ContainingSemantic.Symbol.ContainingType )
                             && context.RewritingDriver.InjectionRegistry.IsOverrideTarget( targetSymbol )
                        => LinkerRewritingDriver.GetOriginalImplParameterType(),
                    IntermediateSymbolSemanticKind.Base
                        when SymbolEqualityComparer.Default.Equals(
                            this._aspectReference.ResolvedSemantic.Symbol.ContainingType,
                            this._aspectReference.ContainingSemantic.Symbol.ContainingType )
                        => LinkerRewritingDriver.GetEmptyImplParameterType(),
                    _ when context.RewritingDriver.InjectionRegistry.IsOverride( targetSymbol ) =>
                        this.GetExistingIndexerAspectParameterType(),
                    _ => null
                };

            // Presume that all aspect reference symbol source nodes are member access expressions or conditional access expressions.
            switch ( currentNode )
            {
                case ElementAccessExpressionSyntax elementAccessExpression:
                    // Access to an indexer.
                    if ( elementAccessExpression is not { Expression.RawKind: (int) SyntaxKind.ThisExpression } )
                    {
                        throw new AssertionFailedException( $"{elementAccessExpression.Expression.Kind()} element access is not supported." );
                    }

                    if ( SymbolEqualityComparer.Default.Equals(
                            this._aspectReference.ContainingSemantic.Symbol.ContainingType,
                            targetSymbol.ContainingType ) )
                    {
                        return elementAccessExpression
                            .WithArgumentList( this.GetRewrittenIndexerArguments( elementAccessExpression.ArgumentList, targetMemberAspectParameterType ) );
                    }
                    else
                    {
                        if ( this.CompilationContext.SymbolComparer.Is(
                                targetSymbol.ContainingType,
                                this._aspectReference.ContainingSemantic.Symbol.ContainingType ) )
                        {
                            throw new AssertionFailedException( "Resolved symbol is declared in a derived class." );
                        }
                        else if ( this.CompilationContext.SymbolComparer.Is(
                                     this._aspectReference.ContainingSemantic.Symbol.ContainingType,
                                     targetSymbol.ContainingType ) )
                        {
                            // Resolved symbol is declared in a base class.
                            switch (targetSymbol, elementAccessExpression.Expression)
                            {
                                case (_, IdentifierNameSyntax):
                                case (_, BaseExpressionSyntax):
                                case (_, ThisExpressionSyntax):
                                    return
                                        ElementAccessExpression(
                                                BaseExpression(),
                                                this.GetRewrittenIndexerArguments( elementAccessExpression.ArgumentList, targetMemberAspectParameterType ) )
                                            .WithLeadingTrivia( elementAccessExpression.GetLeadingTrivia() )
                                            .WithTrailingTrivia( elementAccessExpression.GetTrailingTrivia() );

                                default:
                                    var aspectInstance = this.ResolveAspectInstance( context );

                                    var targetDeclaration = aspectInstance.TargetDeclaration.GetSymbol( context.RewritingDriver.IntermediateCompilation )
                                        .AssertNotNull();

                                    context.RewritingDriver.DiagnosticSink.Report(
                                        AspectLinkerDiagnosticDescriptors.CannotUseBaseInvokerWithNonInstanceExpression.CreateRoslynDiagnostic(
                                            targetDeclaration.GetLocationForDiagnostic(),
                                            (aspectInstance.AspectClass.ShortName, TargetDeclaration: targetDeclaration) ) );

                                    return elementAccessExpression;
                            }
                        }
                        else
                        {
                            // Resolved symbol is unrelated to the containing symbol.
                            return elementAccessExpression
                                .WithArgumentList( this.GetRewrittenIndexerArguments( elementAccessExpression.ArgumentList, targetMemberAspectParameterType ) );
                        }
                    }

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
            }
        }

        private IAspectInstanceInternal ResolveAspectInstance( SubstitutionContext context )
        {
            var injectedMember = context.RewritingDriver.InjectionRegistry.GetInjectedMemberForSymbol( this._aspectReference.ContainingSemantic.Symbol );

            return injectedMember.AssertNotNull().Transformation.ParentAdvice.Aspect;
        }

        private TypeSyntax GetExistingIndexerAspectParameterType()
        {
            var targetIndexer = (IPropertySymbol) this._aspectReference.ResolvedSemantic.Symbol;
            IParameterSymbol aspectParameter;

            if ( targetIndexer.Parameters.Last().IsParams )
            {
                aspectParameter = targetIndexer.Parameters[targetIndexer.Parameters.Length - 2];
            }
            else
            {
                aspectParameter = targetIndexer.Parameters[targetIndexer.Parameters.Length - 1];
            }

            return OurSyntaxGenerator.Default.Type( aspectParameter.Type );
        }

        private BracketedArgumentListSyntax GetRewrittenIndexerArguments( BracketedArgumentListSyntax arguments, TypeSyntax? aspectParameterType )
        {
            if ( aspectParameterType != null )
            {
                var targetIndexer = (IPropertySymbol) this._aspectReference.OriginalSymbol;

                int argumentInsertIndex;

                if ( targetIndexer.Parameters.Last().IsParams )
                {
                    argumentInsertIndex = targetIndexer.Parameters.Length - 1;
                }
                else
                {
                    argumentInsertIndex = targetIndexer.Parameters.Length;
                }

                return arguments
                    .WithArguments(
                        arguments.Arguments.Insert(
                            argumentInsertIndex,
                            Argument( DefaultExpression( aspectParameterType ) ) ) );
            }
            else
            {
                return arguments;
            }
        }
    }
}