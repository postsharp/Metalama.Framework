// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;

namespace Metalama.Framework.DesignTime.CodeLens;

public sealed class CodeLensDetailsField : ICodeLensDetailsField
{
    public CodeLensDetailsField( string text )
    {
        this.Text = text;
    }

    public string Text { get; }
}