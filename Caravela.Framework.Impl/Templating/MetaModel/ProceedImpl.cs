using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    class ProceedImpl : IProceedImpl
    {
        private readonly MethodDeclarationSyntax _method;

        public ProceedImpl(MethodDeclarationSyntax method)
        {
            this._method = method;
        }

        public TypeSyntax CreateTypeSyntax()
        {
            if (this._method.ReturnType is PredefinedTypeSyntax predefinedType &&
                predefinedType.Keyword.Kind() == SyntaxKind.VoidKeyword)
            {
                return IdentifierName("__Void");
            }
            else
            {
                return this._method.ReturnType;
            }
        }

        public StatementSyntax CreateAssignStatement(string returnValue)
        {
            var returnCounter = new CountReturnStatements();
            returnCounter.Visit(this._method.Body);
            if (returnCounter.Count == 0)
            {
                return this._method.Body;
            }
            else if (returnCounter.Count == 1 && this.IsLastStatement(this._method, returnCounter.LastReturnStatement))
            {
                // There is a single return statement at the end. We don't need to generate the label and the goto.
                
                var rewriter = new ReturnToAssignmentRewriter(returnValue,null);
                
                return (BlockSyntax) rewriter.Visit(this._method.Body);
                
            }
            else
            {
                var rewriter = new ReturnToAssignmentRewriter(returnValue,"__continue");
                
                var body = (BlockSyntax) rewriter.Visit(this._method.Body);
                
                return Block(
                    body,
                    LabeledStatement("__continue", EmptyStatement())
                );

            }
        }

        private bool IsLastStatement(MethodDeclarationSyntax method, SyntaxNode node)
        {
            if (node.Parent == method.Body)
            {
                // Termination of the loop.
                return true;
            }
            else
            {
                if (node.Parent is BlockSyntax parentBlock && parentBlock.Statements.Last() == node)
                {
                    return this.IsLastStatement(method, parentBlock);
                }
                else
                {
                    return false;
                }
                
            }
            
        }

        public StatementSyntax CreateReturnStatement()
        {
            return this._method.Body;
        }

        private class CountReturnStatements : CSharpSyntaxWalker
        {
            public int Count { get; private set; }
            public ReturnStatementSyntax LastReturnStatement { get; private set; }
            
            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                this.Count++;
                this.LastReturnStatement = node;
                base.VisitReturnStatement(node);
            }
        }

        private class ReturnToAssignmentRewriter : CSharpSyntaxRewriter
        {
            private readonly string _returnValueName;
            private readonly string? _returnLabelName;

            public ReturnToAssignmentRewriter(string returnValueName, string? returnLabelName)
            {
                this._returnValueName = returnValueName;
                this._returnLabelName = returnLabelName;
            }

            public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
            {
                if (node.Expression != null)
                {
                    var assignment = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(this._returnValueName), node.Expression));

                    if (this._returnLabelName != null)
                    {
                        return Block(

                            assignment,
                            GotoStatement(SyntaxKind.GotoStatement, IdentifierName(this._returnLabelName))
                        );
                    }
                    else
                    {
                        return assignment;
                    }
                }
                else
                {
                    if (this._returnLabelName != null)
                    {
                        return Block(

                            GotoStatement(SyntaxKind.GotoStatement, IdentifierName(this._returnLabelName))
                        );
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