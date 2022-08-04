﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReference
{
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public ICompilationVersion CompilationVersion { get; }

    public ulong CompileTimeProjectHash { get; }

    public DesignTimeCompilationReference(
        ITransitiveAspectsManifest? transitiveAspectsManifest,
        ICompilationVersion compilationVersion,
        ulong compileTimeProjectHash )
    {
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.CompilationVersion = compilationVersion;
        this.CompileTimeProjectHash = compileTimeProjectHash;
    }
}