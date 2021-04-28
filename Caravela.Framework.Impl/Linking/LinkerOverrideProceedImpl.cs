// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerOverrideProceedImpl : IProceedImpl
    {
        private readonly IMethod _originalDeclaration;
        private readonly AspectLayerId _aspectLayerId;
        private readonly ISyntaxFactory _syntaxFactory;

        public LinkerOverrideProceedImpl( AspectLayerId aspectLayerId, IMethod overridenDeclaration, ISyntaxFactory syntaxFactory )
        {
            this._aspectLayerId = aspectLayerId;
            this._originalDeclaration = overridenDeclaration;
            this._syntaxFactory = syntaxFactory;
        }

        TypeSyntax IProceedImpl.CreateTypeSyntax()
        {
            if ( this._originalDeclaration.ReturnType.Is( typeof(void) ) )
            {
                // TODO: Add the namespace.
                return this._syntaxFactory.GetTypeNameSyntax( typeof(__Void) );
            }

            // TODO: Introduced types?
            return (TypeSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( (ITypeSymbol) ((NamedType) this._originalDeclaration.ReturnType).Symbol );
        }

        StatementSyntax IProceedImpl.CreateAssignStatement( SyntaxToken returnValueLocalName )
        {
            // Emit `xxx = <original_method_call>`.
            return
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName( returnValueLocalName ),
                        this.CreateOriginalMethodCall() ) );
        }

        StatementSyntax IProceedImpl.CreateReturnStatement()
        {
            if ( this._originalDeclaration.ReturnType.Is( typeof(void) ) )
            {
                // Emit `<original_method_call>; return`.
                return Block(
                    ExpressionStatement( this.CreateOriginalMethodCall() ),
                    ReturnStatement() );
            }

            // Emit `return <original_method_call>`.
            return
                ReturnStatement( this.CreateOriginalMethodCall() );
        }

        private InvocationExpressionSyntax CreateOriginalMethodCall()
        {
            // Emit `OriginalMethod( a, b, c )` where `a, b, c` is the canonical list of arguments.
            // TODO: generics, static methods, consider explicit, modifiers interfaces and other special methods.
            var invocation =
                InvocationExpression(
                    !this._originalDeclaration.IsStatic
                        ? MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( this._originalDeclaration.Name ) )
                        : IdentifierName( this._originalDeclaration.Name ),
                    ArgumentList( SeparatedList( this._originalDeclaration.Parameters.Select( x => Argument( IdentifierName( x.Name! ) ) ) ) ) );

            invocation = invocation.AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, LinkerAnnotationOrder.Default ) );

            return invocation;
        }

        // The following commented logic should move to the aspect linker.

        /*
        StatementSyntax IProceedImpl.CreateAssignStatement( string returnLocalVariableName )
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

                var rewriter = new ReturnToAssignmentRewriter( returnLocalVariableName, null );

                return (BlockSyntax) rewriter.Visit( methodBody );
            }
            else
            {
                var rewriter = new ReturnToAssignmentRewriter( returnLocalVariableName, "__continue" );

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

        StatementSyntax IProceedImpl.CreateReturnStatement()
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
        */
    }
}