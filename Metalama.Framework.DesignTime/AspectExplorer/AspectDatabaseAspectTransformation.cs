// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.AspectExplorer;

public class AspectDatabaseAspectTransformation( string targetDeclarationId, string description )
{
    public string TargetDeclarationId { get; } = targetDeclarationId;

    public string Description { get; } = description;
}