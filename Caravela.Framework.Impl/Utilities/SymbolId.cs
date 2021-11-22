// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

extern alias roslyn;
using Microsoft.CodeAnalysis;
using System.Threading;
using SymbolKey = roslyn::Microsoft.CodeAnalysis.SymbolKey;
using SymbolKeyExtensions = roslyn::Microsoft.CodeAnalysis.SymbolKeyExtensions;

// ReSharper disable once RedundantBlankLines

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// An identifier of an <see cref="ISymbol"/> that works across compilations, but not across different versions of Roslyn.  
    /// </summary>
    internal struct SymbolId
    {
#pragma warning disable IDE0044 // SymbolKey.Resolve is mutating.
        private SymbolKey _symbolKey;
#pragma warning restore IDE0044

        public SymbolId( string id )
        {
            this._symbolKey = new SymbolKey( id );
        }

        private SymbolId( SymbolKey symbolKey )
        {
            this._symbolKey = symbolKey;
        }

        public ISymbol? Resolve( Compilation compilation, bool ignoreAssemblyKey = false, CancellationToken cancellationToken = default )
            => this._symbolKey.Resolve( compilation, ignoreAssemblyKey, cancellationToken ).Symbol;

        public override string ToString() => this._symbolKey.ToString();

        public static SymbolId Create( ISymbol? symbol, CancellationToken cancellationToken = default )
        {
            if ( symbol == null )
            {
                return default;
            }
            else
            {
                // ReSharper disable once InvokeAsExtensionMethod
                var symbolKey = SymbolKeyExtensions.GetSymbolKey( symbol, cancellationToken );

                return new SymbolId( symbolKey );
            }
        }
    }
}