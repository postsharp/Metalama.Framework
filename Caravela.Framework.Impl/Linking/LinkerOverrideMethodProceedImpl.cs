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

    internal class LinkerOverrideMethodProceedImpl : IProceedImpl
    {
        private readonly IMethod _overriddenDeclaration;
        private readonly AspectLayerId _aspectLayerId;
        private readonly LinkerAnnotationOrder _order;
        private readonly ISyntaxFactory _syntaxFactory;

        public LinkerOverrideMethodProceedImpl( AspectLayerId aspectLayerId, IMethod overriddenDeclaration, LinkerAnnotationOrder order, ISyntaxFactory syntaxFactory )
        {
            this._aspectLayerId = aspectLayerId;
            this._overriddenDeclaration = overriddenDeclaration;
            this._order = order;
            this._syntaxFactory = syntaxFactory;
        }

        TypeSyntax IProceedImpl.CreateTypeSyntax()
        {
            // TODO: Introduced types?
            if ( this._overriddenDeclaration.ReturnType.Is( typeof( void ) ) )
            {
                return this._syntaxFactory.GetTypeNameSyntax( typeof( __Void ) );
            }

            // TODO: Introduced types?
            return (TypeSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( (ITypeSymbol) ((NamedType) this._overriddenDeclaration.ReturnType).Symbol );
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
            if ( this._overriddenDeclaration.ReturnType.Is( typeof( void ) ) )
            {
                // Emit `{ <original_method_call>; return; }`.
                return
                    Block(
                        ExpressionStatement( this.CreateOriginalMethodCall() ),
                        ReturnStatement() );
            }
            else
            {
                // Emit `return <original_method_call>`.
                return
                    ReturnStatement( this.CreateOriginalMethodCall() );
            }
        }

        private InvocationExpressionSyntax CreateOriginalMethodCall()
        {
            // Emit `OriginalMethod( a, b, c )` where `a, b, c` is the canonical list of arguments.
            // TODO: generics, static methods, consider explicit, modifiers interfaces and other special methods.
            var invocation =
                InvocationExpression(
                    !this._overriddenDeclaration.IsStatic
                        ? MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( this._overriddenDeclaration.Name ) )
                        : IdentifierName( this._overriddenDeclaration.Name ),
                    ArgumentList( SeparatedList( this._overriddenDeclaration.Parameters.Select( x => Argument( IdentifierName( x.Name! ) ) ) ) ) );

            invocation = invocation.AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, this._order ) );

            return invocation;
        }
    }
}