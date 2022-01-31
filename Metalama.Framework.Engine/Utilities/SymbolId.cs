// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable once RedundantBlankLines

#pragma warning disable SA1516 // Elements should be separated by blank line
extern alias roslyn;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Threading;
using SymbolKeyExtensions = roslyn::Microsoft.CodeAnalysis.SymbolKeyExtensions;

#pragma warning restore SA1516 // Elements should be separated by blank line

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// An identifier of an <see cref="ISymbol"/> that works across compilations, but not across different versions of Roslyn.  
    /// </summary>
    public struct SymbolId
    {
#pragma warning disable IDE0044 // SymbolKey.Resolve is mutating.
        private roslyn::Microsoft.CodeAnalysis.SymbolKey _symbolKey;
#pragma warning restore IDE0044

        public string Id => this._symbolKey.ToString();

        [JsonConstructor]
        public SymbolId( string id )
        {
            this._symbolKey = new roslyn::Microsoft.CodeAnalysis.SymbolKey( id );
        }

        private SymbolId( roslyn::Microsoft.CodeAnalysis.SymbolKey symbolKey )
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

        public bool Equals( SymbolId other ) => this._symbolKey.Equals( other._symbolKey );

        public override bool Equals( object? obj ) => obj is SymbolId other && this.Equals( other );

        public override int GetHashCode() => this._symbolKey.GetHashCode();

        public static bool operator ==( SymbolId left, SymbolId right ) => left.Equals( right );

        public static bool operator !=( SymbolId left, SymbolId right ) => !left.Equals( right );
    }
}