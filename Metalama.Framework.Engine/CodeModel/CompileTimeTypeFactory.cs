// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.CodeModel
{
    // The only class that should use this factory is SystemTypeResolver.
    internal class CompileTimeTypeFactory : IService
    {
        private readonly ConcurrentDictionary<string, Type> _instances = new( StringComparer.Ordinal );

        public Type Get( ITypeSymbol symbol )
            => symbol switch
            {
                IDynamicTypeSymbol => throw new AssertionFailedException(),
                IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => throw new AssertionFailedException(),
                _ => this.Get( symbol.GetSymbolId(), symbol.GetReflectionName() )
            };

        public Type Get( SymbolId symbolKey, string fullMetadataName )
        {
            return this._instances.GetOrAdd( symbolKey.ToString(), id => CompileTimeType.CreateFromSymbolId( new SymbolId( id ), fullMetadataName ) );
        }
    }
}