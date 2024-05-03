// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers
{
    internal sealed class MemberComparer<T> : IEqualityComparer<T>
        where T : class, IMember
    {
        public MemberComparer( IEqualityComparer<ISymbol> symbolComparer )
        {
            this._symbolComparer = symbolComparer;
        }

        private readonly IEqualityComparer<ISymbol> _symbolComparer;

        public bool Equals( T? x, T? y )
        {
            if ( ReferenceEquals( x, y ) )
            {
                return true;
            }
            else if ( x == null || y == null )
            {
                return false;
            }

            if ( x.Name != y.Name )
            {
                return false;
            }

            if ( x.IsStatic != y.IsStatic )
            {
                return false;
            }

            if ( x.DeclarationKind != y.DeclarationKind )
            {
                return false;
            }

            if ( x is IHasParameters xHasParameters )
            {
                var yHasParameters = (IHasParameters) y;

                if ( xHasParameters.Parameters.Count != yHasParameters.Parameters.Count )
                {
                    return false;
                }

                for ( var i = 0; i < xHasParameters.Parameters.Count; i++ )
                {
                    var xParameter = xHasParameters.Parameters[i].Type;
                    var yParameter = yHasParameters.Parameters[i].Type;

                    if ( !this._symbolComparer.Equals(
                            xParameter.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ),
                            yParameter.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ) ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode( T obj )
        {
            var hashCode = default(HashCode);
            hashCode.Add( obj.Name );

            if ( obj is IHasParameters hasParameters )
            {
                foreach ( var parameter in hasParameters.Parameters )
                {
                    hashCode.Add(
                        parameter.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedTypeComparison ),
                        this._symbolComparer );
                }
            }

            return hashCode.ToHashCode();
        }
    }
}