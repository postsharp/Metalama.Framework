using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
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
                        GenericName(
                                Identifier( "List" ) )
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( argument ) ) ) ) ) ) )
                    .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SeparatedList( lt ) ) )
                .NormalizeWhitespace();
        }
    }
}