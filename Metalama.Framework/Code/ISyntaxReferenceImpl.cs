// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;

namespace Metalama.Framework.Code;

internal interface ISyntaxReferenceImpl 
{
    IDiagnosticLocation GetDiagnosticLocation( in SyntaxReference syntaxReference );

    string GetKind( in SyntaxReference syntaxReference );
}