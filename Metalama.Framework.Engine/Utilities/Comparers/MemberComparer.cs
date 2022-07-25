// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers
{
    internal class MemberComparer<T> : IEqualityComparer<T>
        where T : IMember
    {
        private MemberComparer() { }

        public static MemberComparer<T> Instance { get; } = new();

        public bool Equals( T x, T y )
        {
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

                    if ( !SymbolEqualityComparer.Default.Equals( xParameter.GetSymbol(), yParameter.GetSymbol() ) )
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
                    hashCode.Add( parameter.Type.GetSymbol(), SymbolEqualityComparer.Default );
                }
            }

            return hashCode.ToHashCode();
        }
    }
}