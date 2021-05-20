﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
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
        private class PropertySetInliningRewriter : InliningRewriterBase
        {
            private IMethodSymbol ContextAccessor => this.ContextBodyMethod;

            public PropertySetInliningRewriter(
                LinkerAnalysisRegistry referenceRegistry,
                SemanticModel semanticModel,
                IPropertySymbol contextProperty,
                string? returnVariableName = null,
                int? returnLabelId = null )
                : base(
                    referenceRegistry,
                    semanticModel,
                    contextProperty,
                    contextProperty.SetMethod.AssertNotNull(),
                    returnVariableName,
                    returnLabelId ) { }

            public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                // Supported form of inlining:
                // <variable> = <annotated_property_access> = value;
                // <annotated_property_access> = value;

                var propertyAccessNode = node.Right switch
                {
                    AssignmentExpressionSyntax innerAssignment => innerAssignment.Left,
                    _ => node.Left
                };

                var annotation = propertyAccessNode.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    return base.VisitAssignmentExpression( node );
                }

                var targetPropertySymbol = (IPropertySymbol) this.SemanticModel.GetSymbolInfo( propertyAccessNode ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.
                var resolvedSymbol = (IPropertySymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextBodyMethod,
                    targetPropertySymbol,
                    annotation.AssertNotNull() );

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the accessor body.
                    return this.GetInlinedBody( resolvedSymbol, null );
                }
                else
                {
                    // Replace with invocation of the correct override.

                    switch ( propertyAccessNode )
                    {
                        case MemberAccessExpressionSyntax memberAccessExpression:
                            // Instance property.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    node.Right switch
                                    {
                                        AssignmentExpressionSyntax innerAssignment =>
                                            innerAssignment.Update(
                                                ReplaceInstancePropertyAccess( targetPropertySymbol, memberAccessExpression, resolvedSymbol ),
                                                innerAssignment.OperatorToken,
                                                innerAssignment.Right.AssertNotNull() ),
                                        _ => ReplaceInstancePropertyAccess( targetPropertySymbol, memberAccessExpression, resolvedSymbol )
                                    } );

                        case IdentifierNameSyntax identifierExpression:
                            // Static property.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    node.Right switch
                                    {
                                        AssignmentExpressionSyntax innerAssignment =>
                                            innerAssignment.Update(
                                                ReplaceStaticPropertyAccess( targetPropertySymbol, identifierExpression, resolvedSymbol ),
                                                innerAssignment.OperatorToken,
                                                innerAssignment.Right.AssertNotNull() ),
                                        _ => ReplaceStaticPropertyAccess( targetPropertySymbol, identifierExpression, resolvedSymbol )
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

            private BlockSyntax? GetInlinedBody( IPropertySymbol calledProperty, string? returnVariableName )
            {
                var labelId = this.GetNextReturnLabelId();

                // Create the top-most inlining rewriter for the called method.
                var innerRewriter = new PropertySetInliningRewriter( this.AnalysisRegistry, this.SemanticModel, calledProperty, returnVariableName, labelId );
                var declaration = (AccessorDeclarationSyntax) calledProperty.SetMethod.AssertNotNull().DeclaringSyntaxReferences.Single().GetSyntax();

                // Run the inlined method's body through the rewriter.                
                var rewrittenBlock =
                    declaration switch
                    {
                        { Body: not null } => (BlockSyntax) innerRewriter.VisitBlock( declaration.Body ).AssertNotNull(),
                        { ExpressionBody: not null } =>
                            (BlockSyntax) innerRewriter.Visit( Block( ExpressionStatement( declaration.ExpressionBody.Expression ) ) )
                                .AssertNotNull(),              // TODO: Preserve trivias.
                        _ => throw new NotSupportedException() // TODO: Auto-properties.
                    };

                // Mark the block as flattenable (this is the root block so it will not get marked by the inner rewriter).
                rewrittenBlock = rewrittenBlock.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

                if ( this.AnalysisRegistry.HasSimpleReturnControlFlow( this.ContextAccessor ) || (returnVariableName == null) )
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
            private static ExpressionSyntax ReplaceInstancePropertyAccess(
                IPropertySymbol originalSymbol,
                MemberAccessExpressionSyntax memberAccess,
                IPropertySymbol targetSymbol )
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

            private static ExpressionSyntax ReplaceStaticPropertyAccess(
                IPropertySymbol originalSymbol,
                IdentifierNameSyntax identifierExpression,
                IPropertySymbol targetSymbol )
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
                        (ExpressionSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( targetSymbol.ContainingType ),
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