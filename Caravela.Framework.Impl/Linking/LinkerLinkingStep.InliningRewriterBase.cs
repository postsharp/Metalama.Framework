// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {

        /// <summary>
        /// Produces inlined method body. This rewriter is used recursively when inlining call to the previous (inner) transformation.
        /// </summary>
        private abstract class InliningRewriterBase : CSharpSyntaxRewriter
        {
            protected LinkerAnalysisRegistry AnalysisRegistry { get; }

            protected SemanticModel SemanticModel { get; }

            protected string? ReturnVariableName { get; }

            protected int? ReturnLabelId { get; }

            /// <summary>
            /// Gets the logical member this rewriter is working on (method, property or event).
            /// </summary>
            protected ISymbol ContextMember { get; }

            /// <summary>
            /// Gets the actual method this rewriter is working on (method or accessor).
            /// </summary>
            protected IMethodSymbol ContextBodyMethod { get; }

            public InliningRewriterBase(
                LinkerAnalysisRegistry analysisRegistry,
                SemanticModel semanticModel,
                ISymbol contextMember,
                IMethodSymbol contextBodyMethod,
                string? returnVariableName = null,
                int? returnLabelId = null )
            {
                this.AnalysisRegistry = analysisRegistry;
                this.SemanticModel = semanticModel;
                this.ContextMember = contextMember;
                this.ContextBodyMethod = contextBodyMethod;
                this.ReturnVariableName = returnVariableName;
                this.ReturnLabelId = returnLabelId;
            }

            protected static string GetAssignmentVariableName( ExpressionSyntax left )
            {
                switch ( left.Kind() )
                {
                    case SyntaxKind.IdentifierName:
                        return ((IdentifierNameSyntax) left).Identifier.Text;

                    default:
                        throw new NotImplementedException( $"TODO {left.Kind()}" );
                }
            }

            protected int GetNextReturnLabelId() => (this.ReturnLabelId ?? 0) + 1;

            // TODO: Create more contextual return label names.
            protected static string GetReturnLabelName( int returnLabelId ) => $"__aspect_return_{returnLabelId}";

            protected abstract SyntaxNode? VisitReturnedExpression( ExpressionSyntax returnedExpression );


            public override SyntaxNode? VisitExpressionStatement( ExpressionStatementSyntax node )
            {
                var transformedExpression = this.Visit( node.Expression );

                if (transformedExpression == null)
                {
                    return null;
                }
                else if (transformedExpression is not ExpressionSyntax)
                {
                    return transformedExpression;
                }
                else
                {
                    return node.Update( (ExpressionSyntax) transformedExpression, node.SemicolonToken );
                }
            }

            public override SyntaxNode? VisitReturnStatement( ReturnStatementSyntax node )
            {
                // Expected form for inlining:
                // return <annotated_node>;

                var linkerAnnotation = node.Expression?.GetLinkerAnnotation();

                if ( linkerAnnotation != null )
                {
                    // This is an annotated member access, by letting the derived class to visit this, 
                    // we will either get the block with the inlined target body or access to the correct member/base member.

                    var updatedExpression = this.VisitReturnedExpression( node.Expression.AssertNotNull() );

                    if ( updatedExpression == null )
                    {
                        return null;
                    }

                    if ( updatedExpression.Kind() == SyntaxKind.Block )
                    {
                        return updatedExpression;
                    }

                    return node.WithExpression( (ExpressionSyntax) updatedExpression );
                }
                
                // This is a normal return.

                if ( this.ReturnLabelId != null )
                {
                    // We are in the inner inlining case and we have a return label we need to jump to instead of returning.
                    if ( node.Expression != null )
                    {
                        if ( this.ReturnVariableName != null )
                        {
                            if ( this.AnalysisRegistry.HasSimpleReturnControlFlow( this.ContextBodyMethod ) )
                            {
                                return
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName( this.ReturnVariableName ),
                                            node.Expression ) );
                            }
                            else
                            {
                                return
                                    Block(
                                        ExpressionStatement(
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName( this.ReturnVariableName.AssertNotNull() ),
                                                node.Expression ) ),
                                        GotoStatement(
                                            SyntaxKind.GotoStatement,
                                            IdentifierName( GetReturnLabelName( this.ReturnLabelId.Value ) ) ) )
                                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
                            }
                        }
                        else
                        {
                            return node;
                        }
                    }
                    else
                    {
                        if ( this.ReturnVariableName == null )
                        {
                            if ( this.AnalysisRegistry.HasSimpleReturnControlFlow( this.ContextBodyMethod ) )
                            {
                                return null;
                            }
                            else
                            {
                                return
                                    GotoStatement(
                                        SyntaxKind.GotoStatement,
                                        IdentifierName( GetReturnLabelName( this.ReturnLabelId.Value ) ) );
                            }
                        }
                        else
                        {
                            // This happens when a template assigns result into a variable but is then applied on a void method.
                            return
                                GotoStatement(
                                    SyntaxKind.GotoStatement,
                                    IdentifierName( GetReturnLabelName( this.ReturnLabelId.Value ) ) );
                        }
                    }
                }
                else
                {
                    if ( this.ReturnVariableName == null )
                    {
                        return node;
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }
                }
            }

            protected static LabeledStatementSyntax GetStandaloneLabelStatement( string labelName)
            {
                return
                    LabeledStatement( 
                        Identifier( labelName ),
                        ExpressionStatement(
                            IdentifierName(
                                MissingToken( SyntaxKind.IdentifierToken ) ) )
                            .WithSemicolonToken(
                                MissingToken( SyntaxKind.SemicolonToken ) ) );
            }

            // This is currently not needed, because this form is not generated by the template compiler.

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
        }
    }
}