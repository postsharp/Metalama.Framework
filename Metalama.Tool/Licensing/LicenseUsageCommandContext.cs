// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Framework.Engine.Licensing;
using System.Collections.Generic;

namespace Metalama.Tool.Licensing;

internal sealed class LicenseUsageCommandContext : ExtendedCommandContext
{
    public LicenseUsageCommandContext( ExtendedCommandContext context, IReadOnlyList<LicenseConsumptionFile> files ) : base( context )
    {
        this.Files = files;
    }

    public IReadOnlyList<LicenseConsumptionFile> Files { get; }
}