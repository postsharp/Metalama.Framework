// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    // Inlining is supported only for specific code constructions, i.e. places where annotated method call is present:
    //
    //  * <variable> = <annotated_property_access> = value;
    //  * <annotated_property_access> = value;

    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Produces inlined method body. This rewriter is used recursively when inlining call to the previous (inner) transformation.
        /// </summary>
        private class EventInliningRewriter : InliningRewriterBase
        {
            private IMethodSymbol ContextAccessor => this.ContextBodyMethod;

            public EventInliningRewriter(
                LinkerAnalysisRegistry referenceRegistry,
                SemanticModel semanticModel,
                IEventSymbol contextEvent,
                IMethodSymbol accessor,
                string? returnVariableName = null,
                int? returnLabelId = null )
                : base(
                    referenceRegistry,
                    semanticModel,
                    contextEvent,
                    accessor,
                    returnVariableName,
                    returnLabelId ) { }

            public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                // Supported form of inlining:
                // <variable> = <annotated_property_access> += value;
                // <annotated_property_access> += value;

                var eventAccessNode = node.Right switch
                {
                    AssignmentExpressionSyntax innerAssignment => innerAssignment.Left,
                    _ => node.Left
                };

                if ( !eventAccessNode.TryGetAspectReference( out var annotation ) )
                {
                    return base.VisitAssignmentExpression( node );
                }

                var targetEventSymbol = (IEventSymbol) this.SemanticModel.GetSymbolInfo( eventAccessNode ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.
                var resolvedSymbol = (IEventSymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextBodyMethod,
                    targetEventSymbol,
                    annotation );

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the accessor body.
                    return this.GetInlinedBody( resolvedSymbol, null );
                }
                else
                {
                    // Replace with invocation of the correct override.

                    switch ( eventAccessNode )
                    {
                        case MemberAccessExpressionSyntax memberAccessExpression:
                            // Instance event.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    node.Right switch
                                    {
                                        AssignmentExpressionSyntax innerAssignment =>
                                            innerAssignment.Update(
                                                ReplaceInstanceEventAccess( targetEventSymbol, memberAccessExpression, resolvedSymbol ),
                                                innerAssignment.OperatorToken,
                                                innerAssignment.Right.AssertNotNull() ),
                                        _ => ReplaceInstanceEventAccess( targetEventSymbol, memberAccessExpression, resolvedSymbol )
                                    } );

                        case IdentifierNameSyntax:
                            // Static event.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    node.Right switch
                                    {
                                        AssignmentExpressionSyntax innerAssignment =>
                                            innerAssignment.Update(
                                                ReplaceStaticEventAccess( targetEventSymbol, resolvedSymbol ),
                                                innerAssignment.OperatorToken,
                                                innerAssignment.Right.AssertNotNull() ),
                                        _ => ReplaceStaticEventAccess( targetEventSymbol, resolvedSymbol )
                                    } );

                        default:
                            throw new NotImplementedException( $"Cannot inline {node.Right}." );
                    }
                }
            }

            protected override SyntaxNode? VisitReturnedExpression( ExpressionSyntax returnedExpression )
            {
                throw new NotSupportedException();
            }

            private BlockSyntax? GetInlinedBody( IEventSymbol calledEvent, string? returnVariableName )
            {
                var labelId = this.GetNextReturnLabelId();

                var calledAccessor =
                    this.ContextBodyMethod.MethodKind switch
                    {
                        MethodKind.EventAdd => calledEvent.AddMethod!,
                        MethodKind.EventRemove => calledEvent.RemoveMethod!,
                        _ => throw new AssertionFailedException()
                    };

                // Create the top-most inlining rewriter for the called method.
                var innerRewriter = new EventInliningRewriter(
                    this.AnalysisRegistry,
                    this.SemanticModel,
                    calledEvent,
                    calledAccessor,
                    returnVariableName,
                    labelId );

                var declaration = (AccessorDeclarationSyntax) calledAccessor.DeclaringSyntaxReferences.Single().GetSyntax();

                // Run the inlined method's body through the rewriter.
                // TODO: Preserve trivia.
                var rewrittenBlock =
                    declaration switch
                    {
                        { Body: not null } => (BlockSyntax) innerRewriter.VisitBlock( declaration.Body ).AssertNotNull(),
                        { ExpressionBody: not null } =>
                            (BlockSyntax) innerRewriter.Visit( Block( ExpressionStatement( declaration.ExpressionBody.Expression ) ) )
                                .AssertNotNull(),
                        _ => throw new NotSupportedException()
                    };

                // Mark the block as flattenable (this is the root block so it will not get marked by the inner rewriter).
                rewrittenBlock = rewrittenBlock.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

                if ( this.AnalysisRegistry.HasSimpleReturnControlFlow( this.ContextAccessor ) || returnVariableName == null )
                {
                    // This method had simple control flow, we can keep the block as-is
                    return rewrittenBlock;
                }
                else
                {
                    // The method does not have simple control flow - we should expect goto and thus create a label.
                    // TODO: The label should be on the next statement, not on empty statement (but that needs to be done after block flattening).
                    return
                        Block(
                                rewrittenBlock.AssertNotNull(),
                                LabeledStatement( GetReturnLabelName( labelId ), EmptyStatement() ) )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
                }
            }

            /// <summary>
            /// Replaces call target for non-inlineable methods.
            /// </summary>
            /// <param name="originalSymbol">Original symbol targeted by the call.</param>
            /// <param name="memberAccess">Call expression.</param>
            /// <param name="targetSymbol">Target symbol.</param>
            /// <returns></returns>
            private static ExpressionSyntax ReplaceInstanceEventAccess(
                IEventSymbol originalSymbol,
                MemberAccessExpressionSyntax memberAccess,
                IEventSymbol targetSymbol )
            {
                if ( SymbolEqualityComparer.Default.Equals( originalSymbol, targetSymbol ) )
                {
                    // Exact match of the property means we will be accessing the original body.
                    return memberAccess.Update(
                        memberAccess.Expression,
                        memberAccess.OperatorToken,
                        IdentifierName( LinkingRewriter.GetOriginalImplMemberName( targetSymbol.Name ) ) );
                }
                else if ( StructuralSymbolComparer.Signature.Equals( originalSymbol, targetSymbol ) )
                {
                    // HACK: Presumes that same signature (name) means base property access.
                    // TODO: Do this properly.
                    return memberAccess.Update( BaseExpression(), memberAccess.OperatorToken, IdentifierName( targetSymbol.Name ) );
                }
                else
                {
                    return memberAccess.Update( memberAccess.Expression, memberAccess.OperatorToken, IdentifierName( targetSymbol.Name ) );
                }
            }

            private static ExpressionSyntax ReplaceStaticEventAccess(
                IEventSymbol originalSymbol,
                IEventSymbol targetSymbol )
            {
                if ( SymbolEqualityComparer.Default.Equals( originalSymbol, targetSymbol ) )
                {
                    // Exact match of the property means we will be accessing the original body.
                    return IdentifierName( LinkingRewriter.GetOriginalImplMemberName( targetSymbol.Name ) );
                }
                else if ( StructuralSymbolComparer.Signature.Equals( originalSymbol, targetSymbol ) )
                {
                    // HACK: Presumes that same signature (name) means base property access.
                    // TODO: Do this properly.
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        (ExpressionSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( targetSymbol.ContainingType ),
                        IdentifierName( targetSymbol.Name ) );
                }
                else
                {
                    return IdentifierName( targetSymbol.Name );
                }
            }
        }
    }
}