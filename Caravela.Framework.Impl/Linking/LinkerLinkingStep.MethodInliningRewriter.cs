// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    // Inlining is supported only for specific code constructions, i.e. places where annotated method call is present:
    //  * <variable> = <annotated_method_call>;
    //  * return <annotated_method_call>;
    //  * <annotated_method_call>;
    //
    //  Explicitly not supported are expressions outside of expression statement. 

    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Produces inlined method body. This rewriter is used recursively when inlining call to the previous (inner) transformation.
        /// </summary>
        private class MethodInliningRewriter : InliningRewriterBase
        {
            private new IMethodSymbol ContextMember => (IMethodSymbol) base.ContextMember;

            public MethodInliningRewriter(
                LinkerAnalysisRegistry analysisRegistry,
                SemanticModel semanticModel,
                IMethodSymbol contextMethod,
                string? returnVariableName = null,
                int? returnLabelId = null )
                : base( analysisRegistry, semanticModel, contextMethod, contextMethod, returnVariableName, returnLabelId ) { }

            protected override SyntaxNode? VisitReturnedExpression( ExpressionSyntax node )
            {
                // Inlined form:
                //   return <annotated_method_call>;

                // TODO: out, ref parameters.

                if ( !node.TryGetAspectReference( out var annotation ) )
                {
                    // Normal invocation.
                    return node;
                }

                var invocationExpression = (InvocationExpressionSyntax) node;

                // This is an invocation of a void method that can be possibly inlined.
                var targetMethodSymbol = this.SemanticModel.GetSymbolInfo( node ).Symbol.AssertNotNull();

                // If the body is inlineable, inline it.
                var resolvedSymbol = (IMethodSymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextMember,
                    targetMethodSymbol,
                    annotation.AssertNotNull() );

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the method body.
                    return this.GetInlinedMethodBody( resolvedSymbol, null );
                }
                else if ( StructuralSymbolComparer.Default.Equals( this.ContextMember, targetMethodSymbol ) )
                {
                    return invocationExpression.Update(
                        ReplaceCallTarget( (IMethodSymbol) targetMethodSymbol, invocationExpression.Expression, resolvedSymbol ),
                        invocationExpression.ArgumentList );
                }
                else
                {
                    return node;
                }
            }

            public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                // Inlined form:
                //   <variable> = <annotated_method_call>;
                //   _ = <annotated_method_call>;

                if ( !node.Right.TryGetAspectReference(out var annotation ) )
                {
                    return base.VisitAssignmentExpression( node );
                }

                var invocation = (InvocationExpressionSyntax) node.Right;

                // This is an invocation of a non-void method that can be possibly inlined.
                var targetMethodSymbol = this.SemanticModel.GetSymbolInfo( invocation ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.
                var resolvedSymbol = (IMethodSymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextMember,
                    targetMethodSymbol,
                    annotation.AssertNotNull() );

                var overrideTargetSymbol =
                    this.AnalysisRegistry.IsOverride( this.ContextBodyMethod )
                        ? this.AnalysisRegistry.GetOverrideTarget( this.ContextBodyMethod )
                        : this.ContextBodyMethod;

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the method body.
                    return this.GetInlinedMethodBody( resolvedSymbol, GetAssignmentVariableName( node.Left ) );
                }
                else if ( overrideTargetSymbol != null && StructuralSymbolComparer.Default.Equals( overrideTargetSymbol, targetMethodSymbol ) )
                {
                    // Replace with invocation of the correct override.
                    return node.Update(
                        node.Left,
                        node.OperatorToken,
                        invocation.Update(
                            ReplaceCallTarget( (IMethodSymbol) targetMethodSymbol, invocation.Expression, resolvedSymbol ),
                            invocation.ArgumentList ) );
                }
                else
                {
                    return node;
                }
            }

            public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
            {
                // Supports inlining in form:
                //    <annotated_method_call>;

                if ( !node.Expression.TryGetAspectReference(out var annotation) )
                {
                    return base.VisitExpressionStatement( node );
                }

                var invocation = (InvocationExpressionSyntax) node.Expression;

                // This is an invocation of a method that can be possibly inlined.
                var targetMethodSymbol = this.SemanticModel.GetSymbolInfo( invocation ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.
                var resolvedSymbol = (IMethodSymbol) this.AnalysisRegistry.ResolveSymbolReference(
                    this.ContextMember,
                    targetMethodSymbol,
                    annotation.AssertNotNull() );

                var overrideTargetSymbol =
                    this.AnalysisRegistry.IsOverride( this.ContextBodyMethod )
                        ? this.AnalysisRegistry.GetOverrideTarget( this.ContextBodyMethod )
                        : this.ContextBodyMethod;

                if ( this.AnalysisRegistry.IsInlineable( resolvedSymbol ) )
                {
                    // Inline the method body.
                    return this.GetInlinedMethodBody( resolvedSymbol, null );
                }
                else if ( overrideTargetSymbol != null && StructuralSymbolComparer.Default.Equals( overrideTargetSymbol, targetMethodSymbol ) )
                {
                    // Replace with invocation of the correct override.
                    return
                        base.VisitExpressionStatement(
                            node.Update(
                                invocation.Update(
                                    ReplaceCallTarget( (IMethodSymbol) targetMethodSymbol, invocation.Expression, resolvedSymbol ),
                                    invocation.ArgumentList ),
                                node.SemicolonToken ) );
                }
                else
                {
                    return node;
                }
            }

            private BlockSyntax? GetInlinedMethodBody( IMethodSymbol calledMethodSymbol, string? returnVariableName )
            {
                var labelId = this.GetNextReturnLabelId();

                // Create the top-most inlining rewriter for the called method.
                var innerRewriter = new MethodInliningRewriter( this.AnalysisRegistry, this.SemanticModel, calledMethodSymbol, returnVariableName, labelId );
                var declaration = (MethodDeclarationSyntax) calledMethodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                // Run the inlined method's body through the rewriter.
                var rewrittenBlock =
                    (BlockSyntax) innerRewriter.VisitBlock( declaration.Body.AssertNotNull() )
                        .AssertNotNull()
                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

                if ( this.AnalysisRegistry.HasSimpleReturnControlFlow( calledMethodSymbol )
                     || (!calledMethodSymbol.ReturnsVoid && returnVariableName == null) )
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
            /// <param name="expression">Call expression.</param>
            /// <param name="methodSymbol"></param>
            /// <returns></returns>
            private static ExpressionSyntax ReplaceCallTarget( IMethodSymbol originalSymbol, ExpressionSyntax expression, IMethodSymbol methodSymbol )
            {
                if ( expression is MemberAccessExpressionSyntax memberAccess )
                {
                    if ( SymbolEqualityComparer.Default.Equals( originalSymbol, methodSymbol ) )
                    {
                        return memberAccess.Update(
                            memberAccess.Expression,
                            memberAccess.OperatorToken,
                            IdentifierName( LinkingRewriter.GetOriginalImplMemberName( methodSymbol.Name ) ) );
                    }
                    else if ( StructuralSymbolComparer.Signature.Equals( originalSymbol, methodSymbol ) )
                    {
                        // HACK: Presumes that same signature means base method call.
                        // TODO: Do this properly.
                        if ( originalSymbol.IsStatic )
                        {
                            return memberAccess.Update(
                                (ExpressionSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( methodSymbol.ContainingType ),
                                memberAccess.OperatorToken,
                                IdentifierName( methodSymbol.Name ) );
                        }
                        else
                        {
                            return memberAccess.Update( BaseExpression(), memberAccess.OperatorToken, IdentifierName( methodSymbol.Name ) );
                        }
                    }
                    else
                    {
                        return memberAccess.Update( memberAccess.Expression, memberAccess.OperatorToken, IdentifierName( methodSymbol.Name ) );
                    }
                }
                else if ( expression is IdentifierNameSyntax _ )
                {
                    if ( !originalSymbol.IsStatic )
                    {
                        throw new AssertionFailedException();
                    }

                    if ( SymbolEqualityComparer.Default.Equals( originalSymbol, methodSymbol ) )
                    {
                        return IdentifierName( LinkingRewriter.GetOriginalImplMemberName( methodSymbol.Name ) );
                    }
                    else if ( StructuralSymbolComparer.Signature.Equals( originalSymbol, methodSymbol ) )
                    {
                        // HACK: Presumes that a same signature means call to the base method.
                        // TODO: Do this properly.
                        return MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            (ExpressionSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( methodSymbol.ContainingType ),
                            IdentifierName( methodSymbol.Name ) );
                    }
                    else
                    {
                        return IdentifierName( methodSymbol.Name );
                    }
                }
                else
                {
                    throw new AssertionFailedException();
                }
            }
        }
    }
}