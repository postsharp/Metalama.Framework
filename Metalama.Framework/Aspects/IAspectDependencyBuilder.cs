// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Exposes methods that allows to express the dependencies of the aspect. This object is available on the <see cref="IAspectClassBuilder.Dependencies"/>
    /// property of the <see cref="IAspectClassBuilder"/> interface and can be called from implementations of the <see cref="IAspect.BuildAspectClass(IAspectClassBuilder)"/>
    /// method.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectDependencyBuilder
    {
        [Obsolete( "Not implemented." )]
        void RequiresAspect<TAspect>()
            where TAspect : Attribute, IAspect, new();
    }
}