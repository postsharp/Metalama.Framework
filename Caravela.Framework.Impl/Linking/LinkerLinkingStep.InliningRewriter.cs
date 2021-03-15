// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
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
            private static readonly string _inlineableBlockAnnotationId = "AspectLinkerInlineableBlock";

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
                    return updatedExpression.WithAdditionalAnnotations( new SyntaxAnnotation( _inlineableBlockAnnotationId ) );
                }

                return node.Update( this.VisitList( node.AttributeLists ), (ExpressionSyntax) updatedExpression, this.VisitToken( node.SemicolonToken ) );
            }

            /*
            public override SyntaxNode? VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
            {
                // Variable declaration - if there is inlineable call, we need to break this statement into multiple declarations.

                // First go through all declared variables and look if there is a linker annotated invocation.
                bool anyBlockifiedInitializer = false;
                Dictionary<VariableDeclaratorSyntax, SyntaxNode?> rewrittenDeclarators = new Dictionary<VariableDeclaratorSyntax, SyntaxNode?>();
                foreach (var declarator in node.Declaration.Variables)
                {
                    var initializerExpr = declarator.Initializer?.Value;
                    var annotation = initializerExpr?.GetLinkerAnnotation();                    
                    if ( annotation != null )
                    {
                        // This is a linker annotated invocation: var x = ..., >> y = Foo(...) <<, z = ...;
                        // If the target is not inlineable, rewrite the call to the final destination.
                        // If the target is inlineable, break the variable declaration into following block:
                        // var x = ...; 
                        // <return_type> y;
                        // <inlined_body>
                        // var z = ...;
                        var calleeSymbol = this._semanticModel.GetSymbolInfo( initializerExpr.AssertNotNull() ).Symbol.AssertNotNull();

                        // If the body is inlineable, inline it.
                        var resolvedSymbol = (IMethodSymbol) this._analysisRegistry.ResolveSymbolReference( this._contextSymbol, calleeSymbol, annotation );
                        if ( this._analysisRegistry.IsBodyInlineable( resolvedSymbol ) )
                        {
                            // Inline the method body.
                            return this.GetInlinedMethodBody( resolvedSymbol, null );
                        }
                        else
                        {
                            return initializerExpr.Update( this.ReplaceCallTarget( node.Expression, resolvedSymbol ), node.ArgumentList );
                        }

                        if ( updatedInitializerExpression != declarator.Initializer )
                        {
                            if ( updatedInitializerExpression is BlockSyntax )
                            {
                                anyBlockifiedInitializer = true;
                            }

                            rewrittenDeclarators[declarator] = updatedInitializerExpression;
                        }
                    }
                }

                if ( anyBlockifiedInitializer )
                {
                    List<StatementSyntax> variableStatements = new List<StatementSyntax>();

                    foreach ( var declarator in node.Declaration.Variables )
                    {
                        if ( rewrittenDeclarators.TryGetValue( declarator, out SyntaxNode updatedInitializerExpression ) )
                        {
                            if ( updatedInitializerExpression is BlockSyntax blockSyntax )
                            {
                                variableStatements.Add( node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( new[] { declarator.WithInitializer( null ) } ) ) ) );
                                variableStatements.Add( blockSyntax );
                            }
                            else
                            {
                                variableStatements.Add(
                                    node.WithDeclaration(
                                        node.Declaration.WithVariables(
                                            SeparatedList(
                                                new[]
                                                {
                                                    declarator.WithInitializer(
                                                        declarator.Initializer?.WithValue((ExpressionSyntax)updatedInitializerExpression ) )
                                                } ) ) ) );
                            }                            
                        }
                        else
                        {
                            variableStatements.Add( node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( new[] { declarator } ) ) ) );
                        }
                    }

                    return Block( variableStatements ).WithAdditionalAnnotations( new SyntaxAnnotation( _inlineableBlockAnnotationId ) );
                }
                else if ( rewrittenDeclarators.Count > 0 )
                {
                    return node.WithDeclaration(
                        node.Declaration.WithVariables(
                            SeparatedList(
                                node.Declaration.Variables
                                .Select( v =>
                                    rewrittenDeclarators.ContainsKey( v )
                                    ? v.WithInitializer( EqualsValueClause( (ExpressionSyntax) rewrittenDeclarators[v] ) )
                                    : v ) ) ) );
                }
                else
                {
                    return node;
                }
            }*/

            public override SyntaxNode? VisitBlock( BlockSyntax node )
            {
                // TODO: This should move to a separate rewriter and unite with what template compiler currently does (or should do).
                var newSyntax = base.VisitBlock( node );

                if ( newSyntax is BlockSyntax newBlock )
                {
                    var statements = new List<StatementSyntax>();
                    var anyInlined = false;

                    foreach ( var statement in newBlock.Statements )
                    {
                        if ( statement.GetAnnotations( _inlineableBlockAnnotationId ).Any() )
                        {
                            anyInlined = true;
                            statements.AddRange( ((BlockSyntax) statement).Statements );
                        }
                        else
                        {
                            statements.Add( statement );
                        }
                    }

                    if ( anyInlined )
                    {
                        return newBlock.WithStatements( List( statements ) );
                    }
                    else
                    {
                        return newBlock;
                    }
                }
                else
                {
                    return newSyntax;
                }
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
                    return this.GetInlinedMethodBody( resolvedSymbol, null );
                }
                else
                {
                    return node.Update( this.ReplaceCallTarget( (IMethodSymbol) calleeSymbol, node.Expression, resolvedSymbol ), node.ArgumentList );
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
                    return node.Update( node.Left, node.OperatorToken, InvocationExpression( this.ReplaceCallTarget( (IMethodSymbol) calleeSymbol, invocation.Expression, resolvedSymbol ), invocation.ArgumentList ) );
                }
            }

            private BlockSyntax? GetInlinedMethodBody( IMethodSymbol calledMethodSymbol, string? returnVariableName )
            {
                var labelId = this.GetNextReturnLabelId();
                var innerRewriter = new InliningRewriter( this._analysisRegistry, this._semanticModel, calledMethodSymbol, returnVariableName, labelId );
                var declaration = (MethodDeclarationSyntax) calledMethodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                var rewrittenBlock = (BlockSyntax) innerRewriter.VisitBlock( declaration.Body.AssertNotNull() ).AssertNotNull();
                rewrittenBlock = rewrittenBlock.WithAdditionalAnnotations( new SyntaxAnnotation( _inlineableBlockAnnotationId ) );

                if ( this._analysisRegistry.HasSimpleReturn( calledMethodSymbol ) )
                {
                    return rewrittenBlock;
                }
                else
                {
                    return
                        Block(
                            rewrittenBlock.AssertNotNull(),
                            LabeledStatement( this.GetReturnLabelName( labelId ), EmptyStatement() ) )
                        .WithAdditionalAnnotations( new SyntaxAnnotation( _inlineableBlockAnnotationId ) );
                }
            }

            private ExpressionSyntax ReplaceCallTarget( IMethodSymbol originalSymbol, ExpressionSyntax expression, IMethodSymbol methodSymbol )
            {
                var memberAccess = (MemberAccessExpressionSyntax) expression;

                if ( SymbolEqualityComparer.Default.Equals( originalSymbol, methodSymbol ) )
                {
                    return memberAccess.Update( memberAccess.Expression, memberAccess.OperatorToken, IdentifierName( LinkingRewriter.GetOriginalBodyMethodName( methodSymbol.Name ) ) );
                }
                else
                {
                    return memberAccess.Update( memberAccess.Expression, memberAccess.OperatorToken, IdentifierName( methodSymbol.Name ) );
                }
            }

            public override SyntaxNode? VisitReturnStatement( ReturnStatementSyntax node )
            {
                // TODO: ref return etc.

                var linkerAnnotation = node.Expression?.GetLinkerAnnotation();
                if ( linkerAnnotation != null )
                {
                    // This is an annotated invocation. By visiting the expression, we will either get a invocation or a block if the invocation target is inlineable.

                    var updatedExpression = this.Visit( node.Expression );

                    if ( updatedExpression == null )
                    {
                        return null;
                    }

                    if ( updatedExpression.Kind() == SyntaxKind.Block )
                    {
                        return updatedExpression.WithAdditionalAnnotations( new SyntaxAnnotation( _inlineableBlockAnnotationId ) );
                    }

                    return node.WithExpression( (ExpressionSyntax) updatedExpression );
                }

                if ( this._returnLabelId != null )
                {
                    // We are in the inner inlining case and we have a return label we need to jump to instead of returning.

                    if ( this._analysisRegistry.HasSimpleReturn( this._contextSymbol ) )
                    {
                        if ( node.Expression != null )
                        {
                            if ( this._returnVariableName != null )
                            {
                                return ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName( this._returnVariableName ),
                                        node.Expression ) );
                            }
                            else
                            {
                                return null;
                            }
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
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName( this._returnVariableName.AssertNotNull() ),
                                            node.Expression ) ),
                                    GotoStatement(
                                        SyntaxKind.GotoStatement,
                                        IdentifierName( this.GetReturnLabelName( this._returnLabelId.Value ) ) ) );
                        }
                        else
                        {
                            return GotoStatement(
                                SyntaxKind.GotoStatement,
                                IdentifierName( this.GetReturnLabelName( this._returnLabelId.Value ) ) );
                        }
                    }
                }

                return node;
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
