// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed class SymbolIdGenerator
    {
        private static readonly ConditionalWeakTable<Compilation, SymbolIdGenerator> _instances = new();
        private readonly ConcurrentDictionary<ISymbol, string> _symbolsToIds = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<string, ISymbol> _idsToSymbols = new( StringComparer.Ordinal );
        private readonly string _compilationId;
        private long _nextSymbolId;
        private static long _nextCompilationId;

        private SymbolIdGenerator()
        {
            this._compilationId = Interlocked.Increment( ref _nextCompilationId ).ToString( CultureInfo.InvariantCulture );
        }

        public static SymbolIdGenerator GetInstance( Compilation compilation ) => _instances.GetValue( compilation, _ => new SymbolIdGenerator() );

        public string GetId( ISymbol symbol )
        {
            return this._symbolsToIds.GetOrAdd(
                symbol,
                s =>
                {
                    var id =
                        $"node={Interlocked.Increment( ref this._nextSymbolId ).ToString( CultureInfo.InvariantCulture )},compilation={this._compilationId}";

                    this._idsToSymbols[id] = s;

                    return id;
                } );
        }

        public ISymbol GetSymbol( string id ) => this._idsToSymbols[id];

        public override string ToString() => this._compilationId;
    }
}