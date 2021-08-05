// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Code.Collections
{
    /// <summary>
    /// Extension methods for the <see cref="IParameterList"/> class.
    /// </summary>
    public static class ParameterListExtensions
    {
        /// <summary>
        /// Selects the parameters of a given type.
        /// </summary>
        public static IEnumerable<IParameter> OfParameterType<T>( this IParameterList parameters ) => parameters.OfParameterType( typeof(T) );

        /// <summary>
        /// Selects the parameters of a given type.
        /// </summary>
        public static IEnumerable<IParameter> OfParameterType( this IParameterList parameters, Type type )
            => parameters.Where( p => p.ParameterType.Is( type ) );

        /// <summary>
        /// Selects the parameters of a given type.
        /// </summary>
        public static IEnumerable<IParameter> OfParameterType( this IParameterList parameters, IType type )
            => parameters.Where( p => p.ParameterType.Is( type ) );

        public static IParameter? OfName( this IParameterList list, string name ) => list.SingleOrDefault( p => p.Name == name );
    }
}