﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Tool;

internal class VersionCommand : BaseCommand<BaseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, BaseCommandSettings settings )
    {
        context.Console.WriteSuccess( EngineAssemblyMetadataReader.Instance.PackageVersion );
    }
}