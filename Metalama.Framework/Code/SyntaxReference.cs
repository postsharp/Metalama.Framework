// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Code;

public readonly struct SyntaxReference
{
    private readonly ISyntaxReferenceService _syntaxReferenceService;
    public object NodeOrToken { get; }

    internal SyntaxReference( object nodeOrToken, ISyntaxReferenceService syntaxReferenceService )
    {
        this.NodeOrToken = nodeOrToken;
        this._syntaxReferenceService = syntaxReferenceService;
    }

    public IDiagnosticLocation DiagnosticLocation
        => this._syntaxReferenceService.GetDiagnosticLocation( this );
}