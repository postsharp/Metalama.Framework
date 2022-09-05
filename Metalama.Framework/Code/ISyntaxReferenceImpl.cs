// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Code;

internal interface ISyntaxReferenceImpl
{
    IDiagnosticLocation GetDiagnosticLocation( in SyntaxReference syntaxReference );

    string GetKind( in SyntaxReference syntaxReference );
}