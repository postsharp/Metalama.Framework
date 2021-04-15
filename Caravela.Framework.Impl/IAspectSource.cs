// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    internal interface IAspectSource
    {
        AspectSourcePriority Priority { get; }

        IEnumerable<INamedType> AspectTypes { get; }

        IEnumerable<ICodeElement> GetExclusions( INamedType aspectType );

        /// <summary>
        /// Returns a set of <see cref="AspectInstance"/> of a given type. This method is called when the given aspect
        /// type is being processed, not before.
        /// </summary>
        IEnumerable<AspectInstance> GetAspectInstances( INamedType aspectType );
    }
}