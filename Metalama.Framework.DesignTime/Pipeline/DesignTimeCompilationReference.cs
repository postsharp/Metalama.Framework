// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReference
{
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public CompilationVersion CompilationVersion { get; }

    public DesignTimeCompilationReference( ITransitiveAspectsManifest? transitiveAspectsManifest, CompilationVersion compilationVersion )
    {
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.CompilationVersion = compilationVersion;
    }
}