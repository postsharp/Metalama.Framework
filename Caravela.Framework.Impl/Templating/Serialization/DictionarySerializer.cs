using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class DictionarySerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public DictionarySerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        // ReSharper disable once UnusedParameter.Local
        // This method is used so that the C# compiler resolves the generic parameters from 'dynamic'.
        private static object GetDefaultComparer<TK, TV>( Dictionary<TK, TV> dictionary ) => EqualityComparer<TK>.Default;

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
                                        ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( keyType ) ),
                                        Token( SyntaxKind.CommaToken ),
                                        ParseTypeName( TypeNameUtility.ToCSharpQualifiedName( valueType ) ),
                                    } ) ) ) ) );

            dynamic dictionary = o;
            object defaultComparer = GetDefaultComparer( dictionary );
            object actualComparer = dictionary.Comparer;

            if ( keyType == typeof( string ) )
            {
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
                    else if ( sc == StringComparer.InvariantCulture )
                    {
                        comparerName = "InvariantCulture";
                    }
                    else if ( sc == StringComparer.InvariantCultureIgnoreCase )
                    {
                        comparerName = "InvariantCultureIgnoreCase";
                    }
                    else if ( sc.Equals( StringComparer.CurrentCulture ) )
                    {
                        comparerName = "CurrentCulture";
                    }
                    else if ( sc.Equals( StringComparer.CurrentCultureIgnoreCase ) )
                    {
                        comparerName = "CurrentCultureIgnoreCase";
                    }
                    else
                    {
                        // Unknown string comparer
                        throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedDictionaryComparer, sc );
                    }
                }
                else if ( dictionary.Comparer.Equals( EqualityComparer<string>.Default ) )
                {
                    // It's the default string comparer.
                }
                else
                {
                    // Unknown custom comparer for a string-keyed dictionary
                    throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedDictionaryComparer, actualComparer );
                }

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

                    creationExpression = creationExpression.AddArgumentListArguments( Argument( comparerExpression ) );
                }
            }
            else if ( actualComparer != defaultComparer )
            {
                // Unknown custom comparer
                throw new CaravelaException( GeneralDiagnosticDescriptors.UnsupportedDictionaryComparer, actualComparer );
            }

            var lt = new List<InitializerExpressionSyntax>();
            var nonGenericDictionary = (IDictionary) o;
            foreach ( var key in nonGenericDictionary.Keys )
            {
                ThrowIfStackTooDeep( o );

                var value = nonGenericDictionary[key];
                lt.Add( InitializerExpression(
                    SyntaxKind.ComplexElementInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        new SyntaxNodeOrToken[] {
                            this._serializers.SerializeToRoslynCreationExpression( key ),
                            Token(SyntaxKind.CommaToken),
                            this._serializers.SerializeToRoslynCreationExpression( value ) } ) ) );
            }

            creationExpression = creationExpression.WithInitializer(
                InitializerExpression(
                    SyntaxKind.CollectionInitializerExpression,
                    SeparatedList<ExpressionSyntax>( lt ) )
                    )
                .NormalizeWhitespace();
            return creationExpression;
        }
    }
}