// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.DesignTime.Pipeline;

internal readonly struct DesignTimeAspectInstance
{
    public SerializableDeclarationId TargetDeclarationId { get; }

    public string AspectClassFullName { get; }

    public DesignTimeAspectInstance( SerializableDeclarationId targetDeclarationId, string aspectClassFullName )
    {
        this.TargetDeclarationId = targetDeclarationId;
        this.AspectClassFullName = aspectClassFullName;
    }
}