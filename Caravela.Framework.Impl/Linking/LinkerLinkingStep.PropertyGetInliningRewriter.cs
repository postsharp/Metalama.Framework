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
    //  * <variable> = <annotated_property_access>;
    //  * return <annotated_property_access>;

    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Produces inlined method body. This rewriter is used recursively when inlining call to the previous (inner) transformation.
        /// </summary>
        private class PropertyGetInliningRewriter : InliningRewriterBase
        {
            private IMethodSymbol ContextAccessor => this.ContextBodyMethod;

            public PropertyGetInliningRewriter(
                LinkerAnalysisRegistry referenceRegistry,
                SemanticModel semanticModel,
                IPropertySymbol contextProperty,
                string? returnVariableName = null,
                int? returnLabelId = null )
                : base(
                    referenceRegistry,
                    semanticModel,
                    contextProperty,
                    contextProperty.GetMethod.AssertNotNull(),
                    returnVariableName,
                    returnLabelId ) { }

            public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                // Supported form of inlining:
                // <variable> = <annotated_property_access>;

                var annotation = node.Right.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    return base.VisitAssignmentExpression( node );
                }

                var targetPropertySymbol = (IPropertySymbol) this.SemanticModel.GetSymbolInfo( node.Right ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.
                var resolvedSymbol = (IPropertySymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextBodyMethod,
                    targetPropertySymbol,
                    annotation.AssertNotNull() );

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the accessor body.
                    return this.GetInlinedBody( resolvedSymbol, GetAssignmentVariableName( node.Left ) );
                }
                else
                {
                    // Replace with invocation of the correct override.

                    switch ( node.Right )
                    {
                        case MemberAccessExpressionSyntax memberAccessExpression:
                            // Instance property.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    ReplaceInstancePropertyAccess( targetPropertySymbol, memberAccessExpression, resolvedSymbol ) );

                        case IdentifierNameSyntax:
                            // Static property.
                            return
                                node.Update(
                                    node.Left,
                                    node.OperatorToken,
                                    ReplaceStaticPropertyAccess( targetPropertySymbol, resolvedSymbol ) );

                        default:
                            throw new NotImplementedException( $"Cannot inline {node.Right}." );
                    }
                }
            }

            protected override SyntaxNode? VisitReturnedExpression( ExpressionSyntax node )
            {
                // Supported form of inlining:
                // return <annotated_property_access>;

                var annotation = node.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    return node;
                }

                var targetPropertySymbol = (IPropertySymbol) this.SemanticModel.GetSymbolInfo( node ).Symbol.AssertNotNull();

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

                    switch ( node )
                    {
                        case MemberAccessExpressionSyntax memberAccessExpression:
                            // Instance property.
                            return ReplaceInstancePropertyAccess( targetPropertySymbol, memberAccessExpression, resolvedSymbol );

                        case IdentifierNameSyntax:
                            // Static property.
                            return ReplaceStaticPropertyAccess( targetPropertySymbol, resolvedSymbol );

                        default:
                            throw new NotImplementedException( $"Cannot inline {node}." );
                    }
                }
            }

            private BlockSyntax? GetInlinedBody( IPropertySymbol calledProperty, string? returnVariableName )
            {
                var labelId = this.GetNextReturnLabelId();

                // Create the top-most inlining rewriter for the called method.
                var innerRewriter = new PropertyGetInliningRewriter( this.AnalysisRegistry, this.SemanticModel, calledProperty, returnVariableName, labelId );
                var declaration = calledProperty.GetMethod.AssertNotNull().DeclaringSyntaxReferences.Single().GetSyntax();

                // Run the inlined method's body through the rewriter.
                // TODO: Preserve trivias.
                var rewrittenBlock =
                    declaration switch
                    {
                        AccessorDeclarationSyntax { Body: not null } accessorDecl => (BlockSyntax) innerRewriter.VisitBlock( accessorDecl.Body! ).AssertNotNull(),
                        AccessorDeclarationSyntax { ExpressionBody: not null } accessorDecl
                            => (BlockSyntax) innerRewriter.Visit( Block( ReturnStatement( accessorDecl.ExpressionBody!.Expression ) ) ).AssertNotNull(),                  
                        ArrowExpressionClauseSyntax { Expression: not null } arrowExprClause
                            => (BlockSyntax) innerRewriter.Visit( Block( ReturnStatement( arrowExprClause.Expression! ) ) ).AssertNotNull(),
                        AccessorDeclarationSyntax _ when calledProperty.IsAbstract == false
                            => Block( ReturnStatement( GetImplicitBackingFieldAccessExpression( calledProperty ) ) ),
                        _ => throw new NotSupportedException() // TODO: Auto-properties.
                    };

                // Mark the block as flattenable (this is the root block).
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
                                GetStandaloneLabelStatement( GetReturnLabelName( labelId ) ) )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
                }
            }

            /// <summary>
            /// Replaces call target for non-inlineable methods.
            /// </summary>
            /// <param name="originalSymbol">Original symbol targeted by the call.</param>
            /// <param name="memberAccess">Call expression.</param>
            /// <param name="targetSymbol"></param>
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