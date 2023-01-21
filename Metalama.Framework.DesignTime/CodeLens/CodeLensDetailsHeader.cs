// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;

namespace Metalama.Framework.DesignTime.CodeLens;

internal sealed class CodeLensDetailsHeader : ICodeLensDetailsHeader
{
    public CodeLensDetailsHeader( string displayName, string uniqueName, bool isVisible = true, double width = 0 )
    {
        this.DisplayName = displayName;
        this.IsVisible = isVisible;
        this.UniqueName = uniqueName;
        this.Width = width;
    }

    public string DisplayName { get; }

    public bool IsVisible { get; }

    public string UniqueName { get; }

    public double Width { get; }
}