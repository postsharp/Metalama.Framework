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
    internal sealed partial class AspectReferenceRenamingSubstitution : SyntaxNodeSubstitution
    {
        private readonly ResolvedAspectReference _aspectReference;

        public override SyntaxNode TargetNode => this._aspectReference.RootNode;

        public AspectReferenceRenamingSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base(
            compilationContext )
        {
            // Auto properties and event field default semantics should not get here.
            Invariant.AssertNot(
                aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IPropertySymbol property }
                && property.IsAutoProperty() == true );

            Invariant.AssertNot(
                aspectReference.ResolvedSemantic is { Kind: IntermediateSymbolSemanticKind.Default, Symbol: IEventSymbol @event }
                && @event.IsEventField() == true );

            this._aspectReference = aspectReference;
        }

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext context )
        {
            // IMPORTANT: This method needs to always strip trivia if rewriting the existing expression.
            //            Trivia existing around the expression are preserved during substitution.

            var targetSymbol = this._aspectReference.ResolvedSemantic.Symbol;
            var targetSemanticKind = this._aspectReference.ResolvedSemantic.Kind;

            if ( context.RewritingDriver.InjectionRegistry.IsLastOverride( targetSymbol ) )
            {
                throw new AssertionFailedException( Justifications.CoverageMissing );

                // // If something is resolved to the last override, we will point to the target declaration instead.
                // targetSymbol = aspectReference.OriginalSymbol;
                // targetSemanticKind = IntermediateSymbolSemanticKind.Final;
            }

            if ( this._aspectReference.RootNode != this._aspectReference.SymbolSourceNode )
            {
                // Root node is different that symbol source node - this is introduction in form:
                // <helper_type>.<helper_member>(<symbol_source_node>);
                // We need to get to symbol source node.

                currentNode = this._aspectReference.RootNode switch
                {
                    InvocationExpressionSyntax { ArgumentList: { Arguments.Count: 1 } argumentList } =>
                        argumentList.Arguments[0].Expression,
                    _ => throw new AssertionFailedException( $"{this._aspectReference.RootNode.Kind()} is not in a supported form." )
                };
            }

            // Determine the target name. Specifically, handle case when the resolved symbol points to the original implementation.
            var targetMemberName =
                targetSemanticKind switch
                {
                    IntermediateSymbolSemanticKind.Default
                        when SymbolEqualityComparer.Default.Equals(
                                 this._aspectReference.ResolvedSemantic.Symbol.ContainingType,
                                 this._aspectReference.ContainingSemantic.Symbol.ContainingType )
                             && context.RewritingDriver.InjectionRegistry.IsOverrideTarget( targetSymbol )
                        => LinkerRewritingDriver.GetOriginalImplMemberName( targetSymbol ),
                    IntermediateSymbolSemanticKind.Base
                        when SymbolEqualityComparer.Default.Equals(
                            this._aspectReference.ResolvedSemantic.Symbol.ContainingType,
                            this._aspectReference.ContainingSemantic.Symbol.ContainingType )
                        => LinkerRewritingDriver.GetEmptyImplMemberName( targetSymbol ),
                    _ => targetSymbol.Name
                };

            SimpleNameSyntax GetRewrittenName( SimpleNameSyntax name )
                => name switch
                {
                    GenericNameSyntax genericName => genericName.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                    IdentifierNameSyntax _ => name.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                    _ => throw new AssertionFailedException( $"{name.Kind()} is not a supported name." )
                };

            // Presume that all aspect reference symbol source nodes are member access expressions or conditional access expressions.
            switch ( currentNode )
            {
                case MemberAccessExpressionSyntax
                {
                    Expression: IdentifierNameSyntax { Identifier.Text: LinkerInjectionHelperProvider.HelperTypeName },
                    Name.Identifier.Text : LinkerInjectionHelperProvider.FinalizeMemberName
                }:
                    // Finalizer invocation.

                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( targetMemberName ) );

                case MemberAccessExpressionSyntax
                {
                    Expression: IdentifierNameSyntax { Identifier.Text: LinkerInjectionHelperProvider.HelperTypeName },
                    Name.Identifier.Text: var operatorName
                } when SymbolHelpers.GetOperatorKindFromName( operatorName ) != OperatorKind.None:
                    // Operator invocation.

                    return
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            context.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ),
                            IdentifierName( targetMemberName ) );

                case MemberAccessExpressionSyntax memberAccessExpression:
                    // The reference expression is member access.

                    if ( SymbolEqualityComparer.Default.Equals(
                            this._aspectReference.ContainingSemantic.Symbol.ContainingType,
                            targetSymbol.ContainingType ) )
                    {
                        if ( this._aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                        {
                            return memberAccessExpression
                                .WithExpression( ThisExpression() )
                                .WithName( GetRewrittenName( memberAccessExpression.Name ) );
                        }
                        else
                        {
                            // This is the same type, we can just change the identifier in the expression.
                            // TODO: Is the target always accessible?
                            switch ( targetSymbol )
                            {
                                case IMethodSymbol { MethodKind: MethodKind.Destructor }:
                                    return memberAccessExpression
                                        .WithExpression( ThisExpression() )
                                        .WithName( IdentifierName( targetMemberName ) );

                                case IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion }:
                                    return memberAccessExpression
                                        .WithExpression( context.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ) )
                                        .WithName( IdentifierName( targetMemberName ) );

                                default:
                                    return memberAccessExpression
                                        .WithName( GetRewrittenName( memberAccessExpression.Name ) );
                            }
                        }
                    }
                    else
                    {
                        if ( targetSymbol.ContainingType.TypeKind == TypeKind.Interface )
                        {
                            // Overrides are always targeting member defined in the current type.
                            throw new AssertionFailedException( Justifications.CoverageMissing );

                            // return memberAccessExpression
                            //     .WithExpression( ThisExpression() )
                            //     .WithName( IdentifierName( targetMemberName ) );
                        }

                        if ( targetSymbol.IsStatic )
                        {
                            // Static member access where the target is a different type.
                            return
                                MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        context.SyntaxGenerationContext.SyntaxGenerator.Type( targetSymbol.ContainingType ),
                                        GetRewrittenName( memberAccessExpression.Name ) )
                                    .WithLeadingTrivia( memberAccessExpression.GetLeadingTrivia() )
                                    .WithTrailingTrivia( memberAccessExpression.GetTrailingTrivia() );
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
                                switch (targetSymbol, memberAccessExpression.Expression)
                                {
                                    case (IMethodSymbol { MethodKind: MethodKind.Destructor }, _):
                                        return
                                            IdentifierName( "__LINKER_TO_BE_REMOVED__" )
                                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.NullAspectReferenceExpression )
                                                .WithLeadingTrivia( memberAccessExpression.GetLeadingTrivia() )
                                                .WithTrailingTrivia( memberAccessExpression.GetTrailingTrivia() );

                                    case (_, IdentifierNameSyntax):
                                    case (_, BaseExpressionSyntax):
                                    case (_, ThisExpressionSyntax):
                                        return
                                            MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    BaseExpression(),
                                                    GetRewrittenName( memberAccessExpression.Name ) )
                                                .WithLeadingTrivia( memberAccessExpression.GetLeadingTrivia() )
                                                .WithTrailingTrivia( memberAccessExpression.GetTrailingTrivia() );

                                    default:
                                        var aspectInstance = this.ResolveAspectInstance( context );

                                        var targetDeclaration = aspectInstance.TargetDeclaration.GetSymbol( context.RewritingDriver.IntermediateCompilation );

                                        context.RewritingDriver.DiagnosticSink.Report(
                                            AspectLinkerDiagnosticDescriptors.CannotUseBaseInvokerWithNonInstanceExpression.CreateRoslynDiagnostic(
                                                targetDeclaration?.GetLocationForDiagnostic(),
                                                (aspectInstance.AspectClass.ShortName, TargetDeclaration: targetDeclaration) ) );

                                        return memberAccessExpression;
                                }
                            }
                            else
                            {
                                // Resolved symbol is unrelated to the containing symbol.
                                return memberAccessExpression
                                    .WithName( GetRewrittenName( memberAccessExpression.Name ) );
                            }
                        }
                    }

                case ConditionalAccessExpressionSyntax conditionalAccessExpression:
                    if ( SymbolEqualityComparer.Default.Equals(
                            this._aspectReference.ContainingSemantic.Symbol.ContainingType,
                            targetSymbol.ContainingType ) )
                    {
                        if ( this._aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                        {
                            throw new AssertionFailedException( Justifications.CoverageMissing );
                        }
                        else
                        {
                            var rewriter = new ConditionalAccessRewriter( targetMemberName );

                            return (ExpressionSyntax) rewriter.Visit( conditionalAccessExpression )!;
                        }
                    }
                    else
                    {
                        throw new AssertionFailedException( Justifications.CoverageMissing );
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
    }
}