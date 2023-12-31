// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Creates and ensures uniqueness of instances of the <see cref="CompileTimeType"/> class.
    /// </summary>
    internal class CompileTimeTypeFactory : IProjectService
    {
        // The class is intentionally project-scoped even if does not depend on the project because
        // we want the lifetime and scope of this dictionary to be project-scoped.
        // Key is SerializableTypeId for most types. Only type parameters can't be represented using just that, so they use SymbolId.
        private readonly ConcurrentDictionary<string, CompileTimeType> _instances = new( StringComparer.Ordinal );

        public CompileTimeType Get( ITypeSymbol symbol )
            => symbol switch
            {
                IDynamicTypeSymbol => throw new AssertionFailedException( "Cannot get a System.Type for the 'dynamic' type." ),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => throw new AssertionFailedException(
                    "Cannot get a System.Type for the 'dynamic[]' type." ),
                _ => this.Get( ContainsTypeParameters( symbol ) ? symbol.GetSymbolId().Id : symbol.GetSerializableTypeId().Id, symbol )
            };

        private static bool ContainsTypeParameters( ITypeSymbol symbol )
            => symbol switch
            {
                ITypeParameterSymbol => true,
                IDynamicTypeSymbol => false,
                INamedTypeSymbol namedType => namedType.TypeParameters.Any( ContainsTypeParameters ),
                IArrayTypeSymbol arrayType => ContainsTypeParameters( arrayType.ElementType ),
                IPointerTypeSymbol pointerType => ContainsTypeParameters( pointerType.PointedAtType ),
                _ => throw new AssertionFailedException( $"Unexpected symbol {symbol} of type {symbol.GetType()}." )
            };

        private CompileTimeType Get( string id, ITypeSymbol symbolForMetadata )
        {
            return this._instances.GetOrAdd(
                id,
                static ( key, s ) => key.StartsWith( "typeof(", StringComparison.Ordinal )
                    ? CompileTimeType.CreateFromTypeId( new SerializableTypeId( key ), s )
                    : CompileTimeType.CreateFromSymbolId( new SymbolId( key ), s ),
                symbolForMetadata );
        }

        public CompileTimeType Get( SerializableTypeId declarationId, CompileTimeTypeMetadata metadata )
        {
            return this._instances.GetOrAdd(
                declarationId.ToString(),
                static ( id, m ) => CompileTimeType.CreateFromTypeId( new SerializableTypeId( id ), m ),
                metadata );
        }
    }
}