// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [CompileTimeOnly]
    public readonly struct ValidateDeclarationContext
    {
        public IAspectState? AspectState { get; }

        public IDiagnosticSink Diagnostics { get; }

        public IDeclaration ValidatedDeclaration { get; }

        public SyntaxReference Syntax { get; }

        public IDiagnosticLocation DiagnosticLocation => this.Syntax.DiagnosticLocation;

        public ValidateDeclarationContext( IDeclaration validatedDeclaration, IAspectState? aspectState, IDiagnosticSink diagnostics, SyntaxReference syntax )
        {
            this.AspectState = aspectState;
            this.Diagnostics = diagnostics;
            this.ValidatedDeclaration = validatedDeclaration;
            this.Syntax = syntax;
        }
    }
}