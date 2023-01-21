// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeLens;

internal sealed class CodeLensDetailsEntry : ICodeLensDetailsEntry
{
    private readonly ImmutableArray<CodeLensDetailsField> _fields;

    internal CodeLensDetailsEntry( ImmutableArray<CodeLensDetailsField> fields, string? tooltip = null )
    {
        this._fields = fields;
        this.Tooltip = tooltip;
    }

    ICodeLensDetailsField[] ICodeLensDetailsEntry.Fields => this._fields.ToArray<ICodeLensDetailsField>();

    public string? Tooltip { get; }
}