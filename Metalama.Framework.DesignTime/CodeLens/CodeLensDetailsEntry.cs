// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeLens;

public sealed class CodeLensDetailsEntry : ICodeLensDetailsEntry
{
    public CodeLensDetailsEntry( ImmutableArray<CodeLensDetailsField> fields, string? tooltip = null )
    {
        this.Fields = fields;
        this.Tooltip = tooltip;
    }

    public ImmutableArray<CodeLensDetailsField> Fields { get; }

    ICodeLensDetailsField[] ICodeLensDetailsEntry.Fields => this.Fields.ToArray<ICodeLensDetailsField>();

    public string? Tooltip { get; }
}