// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable once RedundantBlankLines

#pragma warning disable SA1516 // Elements should be separated by blank line
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Threading;

#pragma warning restore SA1516 // Elements should be separated by blank line

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// An identifier of an <see cref="ISymbol"/> that works across compilations, but not across different versions of Roslyn.  
    /// </summary>
    public readonly struct SymbolId
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly Func<string, object> _newSymbolKeyFunc;

        private static readonly Func<object, Compilation, bool, CancellationToken, ISymbol> _resolveSymbolKeyFunc;
        private static readonly Func<ISymbol, CancellationToken, object> _getSymbolKeyFunc;

        private readonly object _symbolKey;

        public string Id => this._symbolKey.ToString();

        static SymbolId()
        {
            var symbolKeyType = typeof(AdhocWorkspace).Assembly.GetType( "Microsoft.CodeAnalysis.SymbolKey" );
            var symbolKeyResolutionType = typeof(AdhocWorkspace).Assembly.GetType( "Microsoft.CodeAnalysis.SymbolKeyResolution" );
            var symbolKeyExtensionsType = typeof(AdhocWorkspace).Assembly.GetType( "Microsoft.CodeAnalysis.SymbolKeyExtensions" );

            // Get SymbolKey(string) constructor.
            var symbolKeyConstructor = symbolKeyType.GetConstructor( new[] { typeof(string) } );

            var idParameter = Expression.Parameter( typeof(string), "id" );
            var newSymbolKey = Expression.ConvertChecked( Expression.New( symbolKeyConstructor, idParameter ), typeof(object) );
            _newSymbolKeyFunc = Expression.Lambda<Func<string, object>>( newSymbolKey, idParameter ).Compile();

            // Get SymbolKey.Resolve.
            var symbolKeyResolve = symbolKeyType.GetMethod( "Resolve" );
            var symbolKeyResolutionGetSymbol = symbolKeyResolutionType.GetProperty( "Symbol" );

            var symbolKeyParameter = Expression.Parameter( typeof(object), "symbolKey" );
            var compilationParameter = Expression.Parameter( typeof(Compilation), "compilation" );
            var ignoreAssemblyKeyParameter = Expression.Parameter( typeof(bool), "ignoreAssemblyKey" );
            var cancellationTokenParameter = Expression.Parameter( typeof(CancellationToken), "cancellationToken" );

            var callResolve = Expression.Call(
                Expression.Convert( symbolKeyParameter, symbolKeyType ),
                symbolKeyResolve,
                compilationParameter,
                ignoreAssemblyKeyParameter,
                cancellationTokenParameter );

            var getSymbol = Expression.Property( callResolve, symbolKeyResolutionGetSymbol );

            _resolveSymbolKeyFunc = Expression.Lambda<Func<object, Compilation, bool, CancellationToken, ISymbol>>(
                    getSymbol,
                    symbolKeyParameter,
                    compilationParameter,
                    ignoreAssemblyKeyParameter,
                    cancellationTokenParameter )
                .Compile();

            // Get SymbolKeyExtensions.GetSymbolKey
            var symbolParameter = Expression.Parameter( typeof(ISymbol), "symbol" );
            var getSymbolKeyMethod = symbolKeyExtensionsType.GetMethod( "GetSymbolKey" );

            var callGetSymbolKey = Expression.Convert(
                Expression.Call( null, getSymbolKeyMethod, symbolParameter, cancellationTokenParameter ),
                typeof(object) );

            _getSymbolKeyFunc = Expression.Lambda<Func<ISymbol, CancellationToken, object>>( callGetSymbolKey, symbolParameter, cancellationTokenParameter )
                .Compile();
        }

        [JsonConstructor]
        public SymbolId( string id )
        {
            this._symbolKey = _newSymbolKeyFunc( id );
        }

        private SymbolId( object symbolKey )
        {
            this._symbolKey = symbolKey;
        }

        public ISymbol? Resolve( Compilation compilation, bool ignoreAssemblyKey = false, CancellationToken cancellationToken = default )
            => _resolveSymbolKeyFunc( this._symbolKey, compilation, ignoreAssemblyKey, cancellationToken );

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
                var symbolKey = _getSymbolKeyFunc( symbol, cancellationToken );

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