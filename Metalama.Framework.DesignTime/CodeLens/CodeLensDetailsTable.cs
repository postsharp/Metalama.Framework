// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeLens;

public sealed class CodeLensDetailsTable : ICodeLensDetailsTable
{
    private readonly ImmutableArray<CodeLensDetailsHeader> _headers;
    private readonly ImmutableArray<CodeLensDetailsEntry> _entries;

    public static CodeLensDetailsTable Empty { get; } = new( ImmutableArray<CodeLensDetailsHeader>.Empty, ImmutableArray<CodeLensDetailsEntry>.Empty );

    internal CodeLensDetailsTable( ImmutableArray<CodeLensDetailsHeader> headers, ImmutableArray<CodeLensDetailsEntry> entries )
    {
        this._headers = headers;
        this._entries = entries;
    }

    ICodeLensDetailsHeader[] ICodeLensDetailsTable.Headers => this._headers.ToArray<ICodeLensDetailsHeader>();

    ICodeLensDetailsEntry[] ICodeLensDetailsTable.Entries => this._entries.ToArray<ICodeLensDetailsEntry>();

    internal static CodeLensDetailsTable CreateError( params string[] messages )
    {
        return new CodeLensDetailsTable(
            ImmutableArray.Create( new CodeLensDetailsHeader( "Error", "Error", true, 1 ) ),
            ImmutableArray.Create( new CodeLensDetailsEntry( messages.SelectAsImmutableArray( m => new CodeLensDetailsField( m ) ) ) ) );
    }
}