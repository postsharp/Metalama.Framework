﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Abstractly exposes the internal <see cref="IAspectRepository"/> interface as a class that is public to the design-time projects.
/// </summary>
public abstract class AspectRepository : IAspectRepository
{
    public abstract AspectRepository WithAspectInstances( IEnumerable<IAspectInstance> aspectInstances );

    public abstract IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
        where T : IAspect;

    public abstract bool HasAspect( IDeclaration declaration, Type aspectType );
}