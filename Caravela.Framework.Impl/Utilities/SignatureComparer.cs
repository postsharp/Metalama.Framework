// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Utilities
{
    internal  class SignatureComparer : IEqualityComparer<IMember>
    {
        private SignatureComparer() { }

        public static SignatureComparer Instance { get; } = new();

        public bool Equals( IMember x, IMember y )
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

        public int GetHashCode( IMember obj )
        {
            throw new NotImplementedException();
        }
    }
}