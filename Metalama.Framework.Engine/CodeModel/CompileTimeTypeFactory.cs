// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    // The only class that should use this factory is SystemTypeResolver.
    internal partial class CompileTimeTypeFactory
    {
        private readonly ConcurrentDictionary<string, Type> _instances = new( StringComparer.Ordinal );

        private readonly SerializableTypeIdProvider _serializableTypeIdProvider;

        public CompileTimeTypeFactory( SerializableTypeIdProvider serializableTypeIdProvider )
        {
            this._serializableTypeIdProvider = serializableTypeIdProvider;
        }

        public Type Get( ITypeSymbol symbol )
        {
            return symbol switch
            {
                IDynamicTypeSymbol => throw new AssertionFailedException( "Cannot get a System.Type for the 'dynamic' type." ),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => throw new AssertionFailedException(
                    "Cannot get a System.Type for the 'dynamic[]' type." ),
                _ => this.Get( symbol.GetSymbolId(), symbol.GetReflectionName().AssertNotNull() )
            };
        }

        public Type Get( SymbolId symbolKey, string fullMetadataName )
        {
            return this._instances.GetOrAdd( symbolKey.ToString(), id => CompileTimeType.CreateFromSymbolId( new SymbolId( id ), fullMetadataName ) );
        }

        public Type Get( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? substitutions )
        {
            var originalSymbol = this._serializableTypeIdProvider.ResolveId( typeId );

            if ( originalSymbol == null )
            {
                throw new ArgumentOutOfRangeException( nameof(typeId), $"Cannot resolve the type '{typeId}'" );
            }

            if ( substitutions is { Count: > 0 } )
            {
                var compilation = substitutions.First().Value.GetCompilationModel();
                var originalType = compilation.Factory.GetIType( originalSymbol );
                var rewriter = new TypeParameterRewriter( substitutions );
                var rewrittenTypeSymbol = rewriter.Visit( originalType ).GetSymbol();

                return this.Get( rewrittenTypeSymbol );
            }
            else
            {
                return this.Get( originalSymbol );
            }
        }

        public Type Get( SerializableTypeId typeId )
        {
            var symbol = this._serializableTypeIdProvider.ResolveId( typeId );

            return this.Get( symbol );
        }
    }
}