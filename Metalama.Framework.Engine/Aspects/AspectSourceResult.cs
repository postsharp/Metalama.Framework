// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

internal readonly struct AspectSourceResult
{
    public IEnumerable<AspectInstance> AspectInstances { get; }

    public IEnumerable<Ref<IDeclaration>> Exclusions { get; }

    public IEnumerable<AspectRequirement> Requirements { get; }

    public static AspectSourceResult Empty => new( null );

    public AspectSourceResult(
        IEnumerable<AspectInstance>? aspectInstances,
        IEnumerable<Ref<IDeclaration>>? exclusions = null,
        IEnumerable<AspectRequirement>? requirements = null )
    {
        this.Requirements = requirements ?? Enumerable.Empty<AspectRequirement>();
        this.AspectInstances = aspectInstances ?? Enumerable.Empty<AspectInstance>();
        this.Exclusions = exclusions ?? Enumerable.Empty<Ref<IDeclaration>>();
    }
}