// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Code.Collections
{
    public static class ParameterListExtensions
    {
        public static IEnumerable<IParameter> OfParameterType<T>( this IParameterList parameters ) => parameters.OfParameterType( typeof(T) );

        public static IEnumerable<IParameter> OfParameterType( this IParameterList parameters, Type type )
            => parameters.Where( p => p.ParameterType.Is( type ) );

        public static IEnumerable<IParameter> OfParameterType( this IParameterList parameters, IType type )
            => parameters.Where( p => p.ParameterType.Is( type ) );
    }
}