// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Creates and ensures uniqueness of instances of the <see cref="CompileTimeType"/> class.
    /// </summary>
    internal class CompileTimeTypeFactory : IProjectService
    {
        // The class is intentionally project-scoped even if does not depend the project because
        // we want the lifetime and scope of this dictionary to be project-scoped.
        private readonly ConcurrentDictionary<string, CompileTimeType> _instances = new( StringComparer.Ordinal );

        public CompileTimeType Get( ITypeSymbol symbol )
            => symbol switch
            {
                IDynamicTypeSymbol => throw new AssertionFailedException( "Cannot get a System.Type for the 'dynamic' type." ),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => throw new AssertionFailedException(
                    "Cannot get a System.Type for the 'dynamic[]' type." ),
                _ => this.Get( symbol.GetSymbolId(), symbol )
            };

        private CompileTimeType Get( SymbolId symbolKey, ITypeSymbol symbolForMetadata )
        {
            return this._instances.GetOrAdd( symbolKey.ToString(), id => CompileTimeType.CreateFromSymbolId( new SymbolId( id ), symbolForMetadata ) );
        }
    }
}