// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.CompileTimeContracts;

[PublicAPI]
public static class TypeOfResolver
{
    public static Type Resolve( string id, IReadOnlyDictionary<string, IType>? substitutions = null )
    {
        if ( Resolver == null )
        {
            throw new InvalidOperationException( "The service is not properly initialized." );
        }

        return Resolver( id, substitutions );
    }

    internal static Func<string, IReadOnlyDictionary<string, IType>?, Type>? Resolver { get; set; }
}