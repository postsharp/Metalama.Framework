using System;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.TestFramework.Templating
{
    internal class TestProceedImpl : IProceedImpl
    {
        private readonly BaseMethodDeclarationSyntax _method;

        public TestProceedImpl( BaseMethodDeclarationSyntax method )
        {
            this._method = method;
        }

        public TypeSyntax CreateTypeSyntax()
        {
            if ( this._method is MethodDeclarationSyntax method )
            {
                if ( method.ReturnType is PredefinedTypeSyntax predefinedType &&
                    predefinedType.Keyword.Kind() == SyntaxKind.VoidKeyword )
                {
                    return IdentifierName( "__Void" );
                }
                else
                {
                    return method.ReturnType;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public StatementSyntax CreateAssignStatement( string returnValue )
        {
            if ( this._method.Body == null )
            {
                throw new NotImplementedException( "Expression-bodied methods not implemented." );
            }

            var methodBody = this._method.Body!;

            var returnCounter = new CountReturnStatements();
            returnCounter.Visit( methodBody );
            if ( returnCounter.Count == 0 )
            {
                return methodBody;
            }
            else if ( returnCounter.Count == 1 && this.IsLastStatement( returnCounter.LastReturnStatement! ) )
            {
                // There is a single return statement at the end. We don't need to generate the label and the goto.

                var rewriter = new ReturnToAssignmentRewriter( returnValue, null );

                return (BlockSyntax) rewriter.Visit( methodBody );
            }
            else
            {
                var rewriter = new ReturnToAssignmentRewriter( returnValue, "__continue" );

                var body = (BlockSyntax) rewriter.Visit( methodBody );

                return Block(
                    body,
                    LabeledStatement( "__continue", EmptyStatement() ) );
            }
        }

        private bool IsLastStatement( SyntaxNode node )
        {
            if ( this._method.Body == null )
            {
                throw new NotImplementedException( "Expression-bodied methods not implemented." );
            }

            var methodBody = this._method.Body!;

            if ( node.Parent == methodBody )
            {
                // Termination of the loop.
                return true;
            }
            else
            {
                if ( node.Parent is BlockSyntax parentBlock && parentBlock.Statements.Last() == node )
                {
                    return this.IsLastStatement( parentBlock );
                }
                else
                {
                    return false;
                }
            }
        }

        public StatementSyntax CreateReturnStatement()
        {
            if ( this._method.Body == null )
            {
                throw new NotImplementedException( "Expression-bodied methods not implemented." );
            }

            return this._method.Body!;
        }

        private class CountReturnStatements : CSharpSyntaxWalker
        {
            public int Count { get; private set; }

            public ReturnStatementSyntax? LastReturnStatement { get; private set; }

            public override void VisitReturnStatement( ReturnStatementSyntax node )
            {
                this.Count++;
                this.LastReturnStatement = node;
                base.VisitReturnStatement( node );
            }
        }

        private class ReturnToAssignmentRewriter : CSharpSyntaxRewriter
        {
            private readonly string _returnValueName;
            private readonly string? _returnLabelName;

            public ReturnToAssignmentRewriter( string returnValueName, string? returnLabelName )
            {
                this._returnValueName = returnValueName;
                this._returnLabelName = returnLabelName;
            }

            public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
            {
                if ( node.Expression != null )
                {
                    var assignment = ExpressionStatement( AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, IdentifierName( this._returnValueName ), node.Expression ) );

                    if ( this._returnLabelName != null )
                    {
                        return Block(
                            assignment,
                            GotoStatement( SyntaxKind.GotoStatement, IdentifierName( this._returnLabelName ) ) );
                    }
                    else
                    {
                        return assignment;
                    }
                }
                else
                {
                    if ( this._returnLabelName != null )
                    {
                        return Block(
                            GotoStatement( SyntaxKind.GotoStatement, IdentifierName( this._returnLabelName ) ) );
                    }
                    else
                    {
                        return EmptyStatement();
                    }
                }
            }
        }
    }
}