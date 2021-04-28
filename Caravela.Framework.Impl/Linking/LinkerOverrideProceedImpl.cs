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
        private readonly LinkerAnnotationOrder _order;
        private readonly ISyntaxFactory _syntaxFactory;

        public LinkerOverrideProceedImpl( AspectLayerId aspectLayerId, IMethod overridenDeclaration, LinkerAnnotationOrder order, ISyntaxFactory syntaxFactory )
        {
            this._aspectLayerId = aspectLayerId;
            this._originalDeclaration = overridenDeclaration;
            this._order = order;
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

            invocation = invocation.AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, this._order ) );

            return invocation;
        }
    }
}