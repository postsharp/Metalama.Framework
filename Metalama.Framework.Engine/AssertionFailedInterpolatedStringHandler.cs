// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Metalama.Framework.Engine
{
    /// <summary>
    /// Improves errors by enhancing specific values when creating AssertionFailedException.
    /// </summary>
    [InterpolatedStringHandler]
    internal ref struct AssertionFailedInterpolatedStringHandler
    {
        private readonly StringBuilder _builder;

        public AssertionFailedInterpolatedStringHandler( int literalLength, int formattedCount )
        {
            this._builder = new StringBuilder( literalLength );
        }

        public void AppendLiteral( string s )
        {
            this._builder.Append( s );
        }

        public void AppendFormatted<T>( T? value )
        {
            try
            {
                if ( value is ISymbol symbol )
                {
                    this._builder.Append( FormatSymbol( symbol ) );
                }
                else
                {
                    this._builder.Append( value?.ToString() );
                }
            }
            catch ( Exception e )
            {
                this._builder.AppendInvariant( $"{{'{e}'}}" );
            }
        }

        private static string FormatSymbol(ISymbol? symbol)
        {
            switch ( symbol )
            {
                case null:
                    return "(null)";

                case IParameterSymbol parameterSymbol:
                    return $"{SymbolKind.Parameter}:{parameterSymbol.ContainingSymbol}:{parameterSymbol}{FormatLocations( parameterSymbol!.Locations )}";

                case IErrorTypeSymbol errorSymbol:
                    return $"{SymbolKind.ErrorType}:<{errorSymbol!.CandidateReason}>({string.Join( ",", errorSymbol!.CandidateSymbols.Select( FormatSymbol ) )}){FormatLocations( symbol!.Locations )}";

                default:
                    var symbolString = symbol.ToString();

                    if ( string.IsNullOrEmpty( symbolString ) )
                    {
                        return $"{symbol?.Kind}:({FormatSymbol( symbol!.ContainingSymbol )}).<empty>{FormatLocations( symbol!.Locations )}";
                    }
                    else
                    {
                        return $"{symbol?.Kind}:{symbolString}{FormatLocations( symbol!.Locations )}";
                    }
            }
        }

        private static string FormatLocations(ImmutableArray<Location> locations)
        {
            if ( locations.IsEmpty )
            {
                return "";
            }
            else
            {
                return $":[{locations.First()}]";
            }
        }

        internal string GetFormattedText() => this._builder.ToString();
    }
}