// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerOverrideEventProceedImpl : IProceedImpl
    {
        private readonly IMethod _overriddenDeclaration;
        private readonly AspectLayerId _aspectLayerId;
        private readonly LinkerAnnotationOrder _order;
        private readonly ISyntaxFactory _syntaxFactory;

        public LinkerOverrideEventProceedImpl(
            AspectLayerId aspectLayerId,
            IMethod overriddenDeclaration,
            LinkerAnnotationOrder order,
            ISyntaxFactory syntaxFactory )
        {
            this._aspectLayerId = aspectLayerId;
            this._overriddenDeclaration = overriddenDeclaration;
            this._order = order;
            this._syntaxFactory = syntaxFactory;
        }

        private IEvent ContainingEvent => (IEvent) this._overriddenDeclaration.ContainingElement.AssertNotNull();

        TypeSyntax IProceedImpl.CreateTypeSyntax()
        {
            // TODO: Introduced types?
            return (TypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( (ITypeSymbol) ((NamedType) this.ContainingEvent.EventType).Symbol );
        }

        StatementSyntax IProceedImpl.CreateAssignStatement( SyntaxToken returnValueLocalName )
        {
            switch ( this._overriddenDeclaration.MethodKind )
            {
                case MethodKind.PropertyGet:
                    // Emit `xxx = <original_event_access> += value`.
                    return
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName( returnValueLocalName ),
                                AssignmentExpression(
                                    SyntaxKind.AddAssignmentExpression,
                                    this.CreateOriginalEventAccess(),
                                    IdentifierName( "value" ) ) ) );

                case MethodKind.PropertySet:
                    // Emit `xxx = <original_event_access> -= value`.
                    return
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName( returnValueLocalName ),
                                AssignmentExpression(
                                    SyntaxKind.SubtractAssignmentExpression,
                                    this.CreateOriginalEventAccess(),
                                    IdentifierName( "value" ) ) ) );

                default:
                    throw new AssertionFailedException( $"{this._overriddenDeclaration.MethodKind}" );
            }
        }

        StatementSyntax IProceedImpl.CreateReturnStatement()
        {
            switch ( this._overriddenDeclaration.MethodKind )
            {
                case MethodKind.EventAdd:
                    // Emit `{ <original_event_access> += value; return; }`.
                    return
                        Block(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.AddAssignmentExpression,
                                    this.CreateOriginalEventAccess(),
                                    IdentifierName( "value" ) ) ),
                            ReturnStatement() );

                case MethodKind.EventRemove:
                    // Emit `{ <original_event_access> -= value; return; }`.
                    return
                        Block(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SubtractAssignmentExpression,
                                    this.CreateOriginalEventAccess(),
                                    IdentifierName( "value" ) ) ),
                            ReturnStatement() );

                default:
                    throw new AssertionFailedException( $"{this._overriddenDeclaration.MethodKind}" );
            }
        }

        private ExpressionSyntax CreateOriginalEventAccess()
        {
            var originalEvent = (IEvent) this._overriddenDeclaration.ContainingElement.AssertNotNull();

            // TODO: generics, static methods, consider explicit, modifiers interfaces and other special methods.

            // For properties, emit `[[this.]]OriginalProperty`.
            var expression =
                !originalEvent.IsStatic
                    ? MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( originalEvent.Name ) )
                    : (ExpressionSyntax) IdentifierName( originalEvent.Name );

            return expression.AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, this._order ) );
        }
    }
}