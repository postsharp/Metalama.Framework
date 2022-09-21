﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;

namespace Metalama.Framework.DesignTime.Pipeline;

/// <summary>
/// Associates a <see cref="ProjectVersion"/> and a <see cref="TransitiveAspectsManifest"/>.
/// </summary>
internal readonly struct DesignTimeProjectReference
{
    public ITransitiveAspectsManifest? TransitiveAspectsManifest { get; }

    public IProjectVersion ProjectVersion { get; }

    public DesignTimeProjectReference(
        IProjectVersion projectVersion,
        ITransitiveAspectsManifest? transitiveAspectsManifest = null )
    {
        this.TransitiveAspectsManifest = transitiveAspectsManifest;
        this.ProjectVersion = projectVersion;
    }
}