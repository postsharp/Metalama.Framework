﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        private class InliningRewriter : CSharpSyntaxRewriter
        {
            private readonly LinkerAnalysisRegistry _analysisRegistry;
            private readonly SemanticModel _semanticModel;
            private readonly IMethodSymbol _contextSymbol;
            private readonly string? _returnVariableName;
            private readonly int? _returnLabelId;

            public InliningRewriter( LinkerAnalysisRegistry referenceRegistry, SemanticModel semanticModel, IMethodSymbol contextSymbol, string? returnVariableName = null, int? returnLabelId = null )
            {
                this._analysisRegistry = referenceRegistry;
                this._semanticModel = semanticModel;
                this._contextSymbol = contextSymbol;
                this._returnVariableName = returnVariableName;
                this._returnLabelId = returnLabelId;
            }

            public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
            {
                var updatedExpression = this.Visit( node.Expression );

                if ( updatedExpression == null )
                {
                    return null;
                }

                if ( updatedExpression.Kind() == SyntaxKind.Block )
                {
                    return updatedExpression;
                }

                return node.Update( this.VisitList( node.AttributeLists ), (ExpressionSyntax) updatedExpression, this.VisitToken( node.SemicolonToken ) );
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                // TODO: out, ref parameters.
                var annotation = node.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    // Normal invocation.
                    return base.VisitInvocationExpression( node );
                }

                // We are on an invocation of a void method that can be possibly inlined.     
                var calleeSymbol = this._semanticModel.GetSymbolInfo( node ).Symbol.AssertNotNull();

                // If the body is inlineable, inline it.
                var resolvedSymbol = (IMethodSymbol) this._analysisRegistry.ResolveSymbolReference( this._contextSymbol, calleeSymbol, annotation );
                if ( this._analysisRegistry.IsBodyInlineable( resolvedSymbol ) )
                {
                    // Inline the method body.
                    var innerRewriter = new InliningRewriter( this._analysisRegistry, this._semanticModel, resolvedSymbol, null, this.GetNextReturnLabelId() );
                    var declaration = (MethodDeclarationSyntax) resolvedSymbol.DeclaringSyntaxReferences.Single().GetSyntax();
                    return innerRewriter.VisitBlock( declaration.Body.AssertNotNull() );
                }
                else
                {
                    return node.Update( this.ReplaceCallTarget( node.Expression, resolvedSymbol ), node.ArgumentList );
                }
            }

            public override SyntaxNode? VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                var annotation = node.Right.GetLinkerAnnotation();

                if ( annotation == null )
                {
                    return base.VisitAssignmentExpression( node );
                }

                var invocation = (InvocationExpressionSyntax) node.Right;

                // We are on an invocation of a void method that can be possibly inlined.     
                var calleeSymbol = this._semanticModel.GetSymbolInfo( invocation ).Symbol.AssertNotNull();

                // We are on an assignment of a method return value to a variable.      
                var resolvedSymbol = (IMethodSymbol) this._analysisRegistry.ResolveSymbolReference( this._contextSymbol, calleeSymbol, annotation );
                if ( this._analysisRegistry.IsBodyInlineable( resolvedSymbol ) )
                {
                    // Inline the method body
                    return this.GetInlinedMethodBody( resolvedSymbol, this.GetAssignmentVariableName( node.Left ) );
                }
                else
                {
                    return node.Update( node.Left, node.OperatorToken, InvocationExpression( this.ReplaceCallTarget( invocation.Expression, resolvedSymbol ), invocation.ArgumentList ) );
                }
            }

            private SyntaxNode? GetInlinedMethodBody(IMethodSymbol calledMethodSymbol, string returnVariableName)
            {
                var labelId = this.GetNextReturnLabelId();
                var innerRewriter = new InliningRewriter( this._analysisRegistry, this._semanticModel, calledMethodSymbol, returnVariableName, labelId );
                var declaration = (MethodDeclarationSyntax) calledMethodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                var rewrittenBlock = innerRewriter.VisitBlock( declaration.Body.AssertNotNull() );

                if ( this._analysisRegistry.HasSimpleReturn( this._contextSymbol ) )
                {
                    return rewrittenBlock;
                }
                else
                {
                    return
                        Block(
                            (StatementSyntax)rewrittenBlock.AssertNotNull(),
                            LabeledStatement( this.GetReturnLabelName( labelId ), EmptyStatement() ));
                }
            }

            private ExpressionSyntax ReplaceCallTarget( ExpressionSyntax expression, IMethodSymbol methodSymbol )
            {
                var memberAccess = (MemberAccessExpressionSyntax) expression;

                return memberAccess.Update( memberAccess.Expression, memberAccess.OperatorToken, IdentifierName( methodSymbol.Name ) );
            }

            public override SyntaxNode? VisitReturnStatement( ReturnStatementSyntax node )
            {
                // TODO: ref return etc.

                if ( this._returnLabelId != null )
                {
                    // Inner inlining (i.e. multiple methods inlined into one). Return statements need to be transformed to assign (for non-void method) + jump.

                    if ( this._analysisRegistry.HasSimpleReturn( this._contextSymbol ) )
                    {
                        if ( node.Expression != null )
                        {
                            return ExpressionStatement( AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, IdentifierName( this._returnVariableName.AssertNotNull() ), node.Expression ) );
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {

                        if ( node.Expression != null )
                        {
                            return Block(
                                    ExpressionStatement( AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, IdentifierName( this._returnVariableName.AssertNotNull() ), node.Expression ) ),
                                    GotoStatement( SyntaxKind.GotoStatement, IdentifierName( this.GetReturnLabelName( this._returnLabelId.Value ) ) ) );
                        }
                        else
                        {
                            return GotoStatement( SyntaxKind.GotoStatement, IdentifierName( this.GetReturnLabelName( this._returnLabelId.Value ) ) );
                        }
                    }
                }

                return base.VisitReturnStatement( node );
            }

            private string GetAssignmentVariableName( ExpressionSyntax left )
            {
                switch ( left.Kind() )
                {
                    case SyntaxKind.IdentifierName:
                        return ((IdentifierNameSyntax) left).Identifier.Text;

                    default:
                        throw new NotImplementedException( $"TODO {left.Kind()}" );
                }
            }

            private int GetNextReturnLabelId() => (this._returnLabelId ?? 0) + 1;

            // TODO: Create more contextual return label names.
            private string GetReturnLabelName( int returnLabelId ) => $"__aspect_return_{returnLabelId}";
        }
    }
}
