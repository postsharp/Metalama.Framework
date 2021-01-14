using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    [Obfuscation( Exclude = true )]
    internal class ListSerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public ListSerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        public override ExpressionSyntax SerializeObject( object o )
        {
            Type argument = o.GetType().GetGenericArguments()[0];
            
            List<ExpressionSyntax> lt = new List<ExpressionSyntax>();
            foreach ( var obj in (IEnumerable) o)
            {
                ThrowIfStackTooDeep(obj);
                lt.Add( this._serializers.SerializeToRoslynCreationExpression( obj ) );
            }
            return ObjectCreationExpression(
                QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Collections")),
                            IdentifierName("Generic")),
                        GenericName(
                                Identifier("List"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.ParseTypeName( TypeNameUtility.ToCSharpQualifiedName(argument) )
                                )))))
                    .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SyntaxFactory.SeparatedList( lt ) ) )
                .NormalizeWhitespace(  );
        }
    }
}