// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class ListSerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public ListSerializer( ObjectSerializers serializers )
        {
            this._serializers = serializers;
        }

        public override ExpressionSyntax SerializeObject( object o )
        {
            var argument = o.GetType().GetGenericArguments()[0];

            var lt = new List<ExpressionSyntax>();

            foreach ( var obj in (IEnumerable) o )
            {
                ThrowIfStackTooDeep( obj );
                lt.Add( this._serializers.SerializeToRoslynCreationExpression( obj ) );
            }

            return ObjectCreationExpression(
                    QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName( "System" ),
                                IdentifierName( "Collections" ) ),
                            IdentifierName( "Generic" ) ),
                        GenericName( Identifier( "List" ) )
                            .WithTypeArgumentList(
                                TypeArgumentList( SingletonSeparatedList( ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( argument ) ) ) ) ) ) )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SeparatedList( lt ) ) )
                .NormalizeWhitespace();
        }
    }
}