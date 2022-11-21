// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeLens;

public class CodeLensDetailsTable : ICodeLensDetailsTable
{
    public static CodeLensDetailsTable Empty { get; } = new( ImmutableArray<CodeLensDetailsHeader>.Empty, ImmutableArray<CodeLensDetailsEntry>.Empty );

    public CodeLensDetailsTable( ImmutableArray<CodeLensDetailsHeader> headers, ImmutableArray<CodeLensDetailsEntry> entries )
    {
        this.Headers = headers;
        this.Entries = entries;
    }

    public ImmutableArray<CodeLensDetailsHeader> Headers { get; }

    public ImmutableArray<CodeLensDetailsEntry> Entries { get; }

    ICodeLensDetailsHeader[] ICodeLensDetailsTable.Headers => this.Headers.ToArray<ICodeLensDetailsHeader>();

    ICodeLensDetailsEntry[] ICodeLensDetailsTable.Entries => this.Entries.ToArray<ICodeLensDetailsEntry>();

    public static CodeLensDetailsTable CreateError( params string[] messages )
    {
        return new CodeLensDetailsTable(
            ImmutableArray.Create( new CodeLensDetailsHeader( "Error", "Error", true, 1 ) ),
            ImmutableArray.Create( new CodeLensDetailsEntry( messages.SelectImmutableArray( m => new CodeLensDetailsField( m ) ) ) ) );
    }
}