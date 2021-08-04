// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed class SymbolIdGenerator
    {
        private static readonly ConditionalWeakTable<Compilation, SymbolIdGenerator> _instances = new();
        private readonly ConcurrentDictionary<ISymbol, string> _symbolsToIds = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<string, ISymbol> _idsToSymbols = new( StringComparer.Ordinal );
        private long _nextId;

        private SymbolIdGenerator() { }

        public static SymbolIdGenerator GetInstance( Compilation compilation ) => _instances.GetValue( compilation, _ => new SymbolIdGenerator() );

        public string GetId( ISymbol symbol )
            => this._symbolsToIds.GetOrAdd(
                symbol,
                s =>
                {
                    var id = Interlocked.Increment( ref this._nextId ).ToString( CultureInfo.InvariantCulture );
                    this._idsToSymbols[id] = s;

                    return id;
                } );

        public ISymbol GetSymbol( string id ) => this._idsToSymbols[id];
    }
}