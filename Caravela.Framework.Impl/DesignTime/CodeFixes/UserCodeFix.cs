// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    public class CodeFixInstance
    {
        public CodeFix CodeFix { get; }

        public IDiagnosticDefinition DiagnosticDefinition { get; }

        public Location Location { get; }

        internal CodeFixInstance( IDiagnosticDefinition diagnosticDefinition, Location location, CodeFix codeFix )
        {
            this.DiagnosticDefinition = diagnosticDefinition;
            this.Location = location;
            this.CodeFix = codeFix;
        }
    }
}