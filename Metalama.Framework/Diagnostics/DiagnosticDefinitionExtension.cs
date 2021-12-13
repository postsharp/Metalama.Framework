// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Diagnostics;

public static class DiagnosticDefinitionExtension
{
    public static IDiagnostic CreateDiagnostic( this DiagnosticDefinition definition ) => definition.WithArguments( default );
}