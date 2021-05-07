// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DictionarySerializer : ObjectSerializer
    {
        public DictionarySerializer( SyntaxSerializationService serializers ) : base( serializers ) { }

        // ReSharper disable once UnusedParameter.Local
        // This method is used so that the C# compiler resolves the generic parameters from 'dynamic'.
        private static object GetDefaultComparer<TK, TV>( Dictionary<TK, TV> dictionary ) => EqualityComparer<TK>.Default;

        public override ExpressionSyntax Serialize( object obj, ISyntaxFactory syntaxFactory )
        {
            var dictionaryType = obj.GetType();
            var keyType = dictionaryType.GetGenericArguments()[0];

            var creationExpression = ObjectCreationExpression( syntaxFactory.GetTypeSyntax( dictionaryType ) );

            dynamic dictionary = obj;
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
                        throw SerializationDiagnosticDescriptors.UnsupportedDictionaryComparer.CreateException( sc.GetType() );
                    }
                }
                else if ( dictionary.Comparer.Equals( EqualityComparer<string>.Default ) )
                {
                    // It's the default string comparer.
                }
                else
                {
                    // Unknown custom comparer for a string-keyed dictionary
                    throw SerializationDiagnosticDescriptors.UnsupportedDictionaryComparer.CreateException( actualComparer.GetType() );
                }

                if ( comparerName != null )
                {
                    var comparerExpression = MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            syntaxFactory.GetTypeSyntax( typeof( StringComparer ) ),
                            IdentifierName( comparerName ) )
                        .NormalizeWhitespace();

                    creationExpression = creationExpression.AddArgumentListArguments( Argument( comparerExpression ) );
                }
            }
            else if ( actualComparer != defaultComparer )
            {
                // Unknown custom comparer
                throw SerializationDiagnosticDescriptors.UnsupportedDictionaryComparer.CreateException( actualComparer.GetType() );
            }

            var lt = new List<InitializerExpressionSyntax>();
            var nonGenericDictionary = (IDictionary) obj;

            foreach ( var key in nonGenericDictionary.Keys )
            {
                ThrowIfStackTooDeep( obj );

                var value = nonGenericDictionary[key];

                lt.Add(
                    InitializerExpression(
                        SyntaxKind.ComplexElementInitializerExpression,
                        SeparatedList<ExpressionSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                this.Service.Serialize( key, syntaxFactory ), Token( SyntaxKind.CommaToken ), this.Service.Serialize( value, syntaxFactory )
                            } ) ) );
            }

            creationExpression = creationExpression.WithInitializer(
                    InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SeparatedList<ExpressionSyntax>( lt ) ) )
                .NormalizeWhitespace();

            return creationExpression;
        }

        public override Type InputType => typeof( IReadOnlyDictionary<,> );

        public override Type OutputType => typeof( Dictionary<,> );

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof( IDictionary<,> ) );
    }
}