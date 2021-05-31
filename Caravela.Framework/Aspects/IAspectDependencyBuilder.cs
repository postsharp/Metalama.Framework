// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Validation;
using System;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectDependencyBuilder
    {
        [Obsolete( "Not implemented." )]
        void RequiresAspect<TAspect>()
            where TAspect : Attribute, IAspect, new();
    }
}