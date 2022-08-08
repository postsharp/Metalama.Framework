// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReference
{
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public ICompilationVersion CompilationVersion { get; }

    // For tests only.
    public DesignTimeCompilationReference( ICompilationVersion compilationVersion )
    {
        this.CompilationVersion = compilationVersion;
    }

    public DesignTimeCompilationReference(
        ICompilationVersion compilationVersion,
        ITransitiveAspectsManifest? transitiveAspectsManifest = null )
    {
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.CompilationVersion = compilationVersion;
    }
}