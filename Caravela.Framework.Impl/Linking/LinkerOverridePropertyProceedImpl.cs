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
    internal class LinkerOverridePropertyProceedImpl : IProceedImpl
    {
        private readonly IMethod _overriddenDeclaration;
        private readonly AspectLayerId _aspectLayerId;
        private readonly LinkerAnnotationOrder _order;
        private readonly ISyntaxFactory _syntaxFactory;

        public LinkerOverridePropertyProceedImpl( AspectLayerId aspectLayerId, IMethod overriddenDeclaration, LinkerAnnotationOrder order, ISyntaxFactory syntaxFactory )
        {
            this._aspectLayerId = aspectLayerId;
            this._overriddenDeclaration = overriddenDeclaration;
            this._order = order;
            this._syntaxFactory = syntaxFactory;
        }

        private IProperty ContainingProperty => (IProperty) this._overriddenDeclaration.ContainingElement.AssertNotNull();

        TypeSyntax IProceedImpl.CreateTypeSyntax()
        {
            // TODO: Introduced types?
            return (TypeSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( (ITypeSymbol) ((NamedType) this.ContainingProperty.Type).Symbol );
        }

        StatementSyntax IProceedImpl.CreateAssignStatement( SyntaxToken returnValueLocalName )
        {
            switch ( this._overriddenDeclaration.MethodKind )
            {
                case Code.MethodKind.PropertyGet:
                    // Emit `xxx = <original_property_access>`.
                    return
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName( returnValueLocalName ),
                                this.CreateOriginalPropertyAccess(LinkerAnnotationTargetKind.PropertyGetAccessor) ) );

                case Code.MethodKind.PropertySet:
                    // Emit `xxx = <original_property_access> = value`.
                    return
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName( returnValueLocalName ),
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    this.CreateOriginalPropertyAccess(LinkerAnnotationTargetKind.PropertySetAccessor),
                                    IdentifierName( "value" ) ) ) );

                default:
                    throw new AssertionFailedException( $"{this._overriddenDeclaration.MethodKind}" );
            }
        }

        StatementSyntax IProceedImpl.CreateReturnStatement()
        {
            switch ( this._overriddenDeclaration.MethodKind )
            {
                case Code.MethodKind.PropertyGet:
                    // Emit `return <original_property_access>;`.
                    return
                        ReturnStatement( this.CreateOriginalPropertyAccess( LinkerAnnotationTargetKind.PropertyGetAccessor ) );

                case Code.MethodKind.PropertySet:
                    // Emit `{ <original_property_access> = value; return; }`.
                    return
                        Block(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    this.CreateOriginalPropertyAccess( LinkerAnnotationTargetKind.PropertySetAccessor ),
                                    IdentifierName( "value" ) ) ),
                            ReturnStatement() );

                default:
                    throw new AssertionFailedException( $"{this._overriddenDeclaration.MethodKind}" );
            }
        }

        private ExpressionSyntax CreateOriginalPropertyAccess( LinkerAnnotationTargetKind annotationTargetKind)
        {
            // TODO: static properties, consider explicit, modifiers interfaces and other special methods.

            if (this.ContainingProperty.Parameters.Count > 0)
            {
                Invariant.Assert( !this.ContainingProperty.IsStatic );
                Invariant.Assert( this.ContainingProperty.Name == "Items" );

                // For indexers, emit `this[ a, b, c ]` where `a, b, c` is the canonical list of arguments.
                return 
                    ElementAccessExpression(
                        ThisExpression(),
                        BracketedArgumentList( SeparatedList( this.ContainingProperty.Parameters.Select( x => Argument( IdentifierName( x.Name! ) ) ) ) ) )
                    .AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, this._order, annotationTargetKind ) );
            }
            else
            {
                // For properties, emit `[[this.]]OriginalProperty`.
                var expression =
                    !this.ContainingProperty.IsStatic
                    ? MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this.ContainingProperty.Name ) )
                    : (ExpressionSyntax) IdentifierName( this.ContainingProperty.Name );

                return expression.AddLinkerAnnotation( new LinkerAnnotation( this._aspectLayerId, this._order, annotationTargetKind ) );
            }
        }
    }
}