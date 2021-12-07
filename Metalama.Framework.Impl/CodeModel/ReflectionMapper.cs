// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;

namespace Metalama.Framework.Impl.CodeModel
{
    /// <summary>
    /// Maps System.Reflection objects to Roslyn symbols.
    /// </summary>
    internal class ReflectionMapper
    {
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<Type, ITypeSymbol> _symbolCache = new();

        internal ReflectionMapper( Compilation compilation )
        {
            this._compilation = compilation;
        }

        /// <summary>
        /// Gets a <see cref="INamedTypeSymbol"/> by metadata name.
        /// </summary>
        /// <param name="metadataName"></param>
        public INamedTypeSymbol GetNamedTypeSymbolByMetadataName( string metadataName )
        {
            var symbol = this._compilation.GetTypeByMetadataName( metadataName );

            if ( symbol == null )
            {
                throw GeneralDiagnosticDescriptors.CannotFindType.CreateException( metadataName );
            }

            return symbol;
        }

        /// <summary>
        /// Gets a <see cref="ITypeSymbol"/> given a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITypeSymbol GetTypeSymbol( Type type )
        {
            switch ( type )
            {
                case CompileTimeType compileTimeType:
                    return (ITypeSymbol) compileTimeType.Target.GetSymbol( this._compilation )
                        .AssertNotNull( Justifications.SerializersNotImplementedForIntroductions );

                default:
                    return this._symbolCache.GetOrAdd( type, this.GetTypeSymbolCore );
            }
        }

        private ITypeSymbol GetTypeSymbolCore( Type type )
        {
            if ( type.IsGenericParameter )
            {
                return this.GetNamedTypeSymbol( type.DeclaringType.AssertNotNull(), Array.Empty<Type>() ).TypeParameters[type.GenericParameterPosition];
            }

            if ( type is CompileTimeType compileTimeType )
            {
                return (ITypeSymbol) compileTimeType.Target.GetSymbol( this._compilation ).AssertNotNull( Justifications.TypesAlwaysHaveSymbol );
            }

            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types cannot be represented as Metalama types." );
            }

            if ( type.IsArray )
            {
                var elementType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreateArrayTypeSymbol( elementType, type.GetArrayRank() );
            }

            if ( type.IsPointer )
            {
                var pointedToType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreatePointerTypeSymbol( pointedToType );
            }

            return this.GetNamedTypeSymbol( type, type.GenericTypeArguments );
        }

        private INamedTypeSymbol GetNamedTypeSymbol( Type type, Type[] genericArguments )
        {
            if ( type.DeclaringType != null )
            {
                // In case of nested type, we need to determine the arity from the name. This info is otherwise not exposed.
                var indexOfQuote = type.Name.IndexOf( '`' );

                var arity = 0;

                if ( indexOfQuote >= 0 )
                {
                    var arityString = type.Name.Substring( indexOfQuote + 1 );
                    arity = int.Parse( arityString, CultureInfo.InvariantCulture );
                }

                var declaringTypeGenericArguments = genericArguments.Take( genericArguments.Length - arity ).ToArray();
                var nestedTypeGenericArguments = genericArguments.Skip( declaringTypeGenericArguments.Length ).ToArray();

                var declaringTypeSymbol = this.GetNamedTypeSymbol( type.DeclaringType, declaringTypeGenericArguments );
                var nestedSymbol = declaringTypeSymbol.GetTypeMembers().Single( s => s.MetadataName == type.Name );

                if ( nestedTypeGenericArguments.Length > 0 )
                {
                    nestedSymbol = nestedSymbol.Construct( nestedTypeGenericArguments.Select( this.GetTypeSymbol ).ToArray() );
                }

                return nestedSymbol;
            }
            else if ( genericArguments.Length > 0 )
            {
                var genericDefinition = this.GetNamedTypeSymbolByMetadataName( type.GetGenericTypeDefinition().FullName );
                var genericArgumentSymbols = genericArguments.Select( this.GetTypeSymbol ).ToArray();

                return genericDefinition.Construct( genericArgumentSymbols );
            }
            else
            {
                return this.GetNamedTypeSymbolByMetadataName( type.FullName.AssertNotNull() );
            }
        }
    }
}