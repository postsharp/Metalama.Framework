using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class DictionarySerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public DictionarySerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        public override ExpressionSyntax SerializeObject( object o )
        {
            var dictionaryType = o.GetType();
            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];
            var creationExpression = ObjectCreationExpression(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName( "System" ),
                            IdentifierName( "Collections" ) ),
                        IdentifierName( "Generic" ) ),
                    GenericName(
                            Identifier( "Dictionary" ) )
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( keyType ) ), Token( SyntaxKind.CommaToken ), ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( valueType ) ),
                                    } ) ) ) ) );
            
            if ( keyType == typeof(string) )
            {
                dynamic dictionary = o;
                string? comparerName = null;
                if ( dictionary.Comparer is StringComparer sc )
                {
                    if ( sc == StringComparer.Ordinal )
                    {
                        comparerName = "Ordinal";
                    }
                    else if ( sc == StringComparer.OrdinalIgnoreCase )
                    {
                        comparerName = "OrdinalIgnoreCase";
                    }
                    else if (sc == StringComparer.InvariantCulture)
                    {
                        comparerName = "InvariantCulture";
                    }
                    else if (sc == StringComparer.InvariantCultureIgnoreCase)
                    {
                        comparerName = "InvariantCultureIgnoreCase";
                    }
                    else
                    {
                        // Unknown string comparer
                    }
                }
                else
                {
                    // Unknown custom comparer
                }
                
                // For the unknown comparers, I don't really want to throw an exception because CurrentCulture and even the default comparer aren't easily recognized, so 
                // we would force the user to pick a comparer, when the default comparer is usually good.

                if ( comparerName != null )
                {
                    var comparerExpression = MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName( "System" ),
                                IdentifierName( "StringComparer" ) ),
                            IdentifierName( comparerName ) )
                        .NormalizeWhitespace();

                    creationExpression = creationExpression.WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument( comparerExpression ) ) ) );
                }
            }
            List<SyntaxNodeOrToken> lt = new List<SyntaxNodeOrToken>();
            bool first = true;
            IDictionary nonGenericDictionary = (IDictionary) o;
            foreach ( var key in nonGenericDictionary.Keys)
            {
                if ( !first )
                {
                    lt.Add( Token( SyntaxKind.CommaToken ) );
                }

                try
                {
                    RuntimeHelpers.EnsureSufficientExecutionStack();
                }
                catch
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.CycleInSerialization, o );
                }

                var value = nonGenericDictionary[key];
                lt.Add( InitializerExpression(
                    SyntaxKind.ComplexElementInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        new SyntaxNodeOrToken[]{
                            this._serializers.SerializeToRoslynCreationExpression( key ),
                            Token(SyntaxKind.CommaToken),
                            this._serializers.SerializeToRoslynCreationExpression( value )})) );
                first = false;
            }
            var list = SeparatedList<ExpressionSyntax>( lt );
            creationExpression = creationExpression.WithInitializer(
                InitializerExpression(
                    SyntaxKind.CollectionInitializerExpression, list )
                    )
                .NormalizeWhitespace( );
            return creationExpression;
        }
    }
}