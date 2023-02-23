// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Commands;
using Metalama.Framework.Engine.Licensing;
using System;
using System.Collections.Generic;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class CreditsCommandContext : ExtendedCommandContext
{
    public CreditsCommandContext( ExtendedCommandContext context, IReadOnlyList<LicenseConsumptionFile> files, DateTime horizon ) : base( context )
    {
        this.Files = files;
        this.Horizon = horizon;
    }

    public IReadOnlyList<LicenseConsumptionFile> Files { get; }

    public DateTime Horizon { get; }
}