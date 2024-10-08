// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// Represents an <see cref="Advice"/>, but as a long-lived object, whereas <see cref="Advice"/> is short-lived
/// just designed to create transformations. The <see cref="AdviceInfo"/> class is along to transformations.
/// </summary>
internal class AdviceInfo
{
    public AdviceInfo( Advice.AdviceConstructorParameters parameters )
    {
        this.AspectInstance = parameters.AspectInstance;
        this.AspectLayerId = new AspectLayerId( this.AspectInstance.AspectClass, parameters.LayerName );
        this.SourceCompilation = parameters.SourceCompilation;
    }

    public IAspectInstanceInternal AspectInstance { get; }

    public AspectLayerId AspectLayerId { get; }

    public CompilationModel SourceCompilation { get; }
}