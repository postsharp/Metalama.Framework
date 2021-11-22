// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;


namespace Caravela.Framework.Impl.CodeModel
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
            return this._instances.GetOrAdd( symbolKey.ToString(), id => CompileTimeType.CreateFromSymbolId( new SymbolId(id), fullMetadataName ) );
        }
    }
}