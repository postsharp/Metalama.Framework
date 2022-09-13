// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReference
{
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public ICompilationVersion CompilationVersion { get; }

    public DesignTimeCompilationReference(
        ICompilationVersion compilationVersion,
        ITransitiveAspectsManifest? transitiveAspectsManifest = null )
    {
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.CompilationVersion = compilationVersion;
    }
}