﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code
{
    internal interface IAspectRepository
    {
        /// <summary>
        /// Gets the aspects of a given type on a given declaration.
        /// </summary>
        /// <param name="declaration">The declaration on which the aspects are requested.</param>
        /// <typeparam name="T">The type of aspects.</typeparam>
        /// <returns>The collection of aspects of type <typeparamref name="T"/> on <paramref name="declaration"/>.</returns>
        IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect;

        bool HasAspect( IDeclaration declaration, Type aspectType );
    }
}