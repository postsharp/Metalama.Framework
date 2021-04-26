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
    internal class ReflectionMapper
    {
        private static readonly ConditionalWeakTable<Compilation, ReflectionMapper> _instances = new();
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<Type, ITypeSymbol> _symbolCache = new();
        private readonly ConcurrentDictionary<Type, NameSyntax> _syntaxCache = new();

        private ReflectionMapper( Compilation compilation )
        {
            this._compilation = compilation;
        }

        public static ReflectionMapper GetInstance( Compilation compilation )
        {
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

        public INamedTypeSymbol GetTypeSymbolByReflectionName( string reflectionName )
        {
            var symbol = this._compilation.GetTypeByMetadataName( reflectionName );

            if ( symbol == null )
            {
                throw GeneralDiagnosticDescriptors.CannotFindType.CreateException( reflectionName );
            }

            return symbol;
        }

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
                var genericDefinition = this.GetTypeSymbolByReflectionName( type.GetGenericTypeDefinition().FullName );
                var genericArguments = type.GenericTypeArguments.Select( this.GetTypeSymbol ).ToArray();

                return genericDefinition.Construct( genericArguments! );
            }

            return this.GetTypeSymbolByReflectionName( type.FullName.AssertNotNull() );
        }

        public ITypeSymbol GetTypeSymbol( Type type ) => this._symbolCache.GetOrAdd( type, this.GetTypeSymbolCore );

        public NameSyntax GetTypeNameSyntax( Type type )
            => this._syntaxCache.GetOrAdd( type, t => (NameSyntax) CSharpSyntaxGenerator.Instance.NameExpression( this.GetTypeSymbol( t ) ) );
    }
}