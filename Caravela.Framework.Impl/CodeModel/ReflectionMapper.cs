// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Maps System.Reflection objects to Roslyn symbols.
    /// </summary>
    internal class ReflectionMapper : ISyntaxFactory
    {
        private static readonly ConditionalWeakTable<Compilation, ReflectionMapper> _instances = new();
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<Type, ITypeSymbol> _symbolCache = new();
        private readonly ConcurrentDictionary<Type, NameSyntax> _syntaxCache = new();

        private ReflectionMapper( Compilation compilation )
        {
            this._compilation = compilation;
        }

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public static ReflectionMapper GetInstance( Compilation compilation )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_instances.TryGetValue( compilation, out var value ) )
            {
                lock ( _instances )
                {
                    if ( !_instances.TryGetValue( compilation, out value ) )
                    {
                        value = new ReflectionMapper( compilation );
                        _instances.Add( compilation, value );
                    }
                }
            }

            return value;
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
        public ITypeSymbol GetTypeSymbol( Type type ) => this._symbolCache.GetOrAdd( type, this.GetTypeSymbolCore );

        /// <summary>
        /// Gets a fully-qualified <see cref="NameSyntax"/> for a given reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public NameSyntax GetTypeNameSyntax( Type type )
            => this._syntaxCache.GetOrAdd( type, t => (NameSyntax) CSharpSyntaxGenerator.Instance.NameExpression( this.GetTypeSymbol( t ) ) );

        private ITypeSymbol GetTypeSymbolCore( Type type )
        {
            if ( type is CompileTimeType compileTimeType )
            {
                return compileTimeType.TypeSymbol;
            }

            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types cannot be represented as Caravela types." );
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

            if ( type.IsConstructedGenericType )
            {
                var genericDefinition = this.GetNamedTypeSymbolByMetadataName( type.GetGenericTypeDefinition().FullName );
                var genericArguments = type.GenericTypeArguments.Select( this.GetTypeSymbol ).ToArray();

                return genericDefinition.Construct( genericArguments! );
            }

            return this.GetNamedTypeSymbolByMetadataName( type.FullName.AssertNotNull() );
        }
    }
}