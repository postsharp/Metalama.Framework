// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    [CompileTimeOnly]
    public readonly struct ValidateReferenceContext
    {
        public IAspectState? AspectState { get; }

        public IDiagnosticSink Diagnostics { get; }

        public IDeclaration ReferencedDeclaration { get; }

        public IDeclaration ReferencingDeclaration { get; }

        public INamedType ReferencingType => this.ReferencingDeclaration.GetDeclaringType() ?? throw new InvalidOperationException( $"Don't know how to get the declaring type of '{this.ReferencingDeclaration}'." );

   

        public ValidatedReferenceKinds ReferenceKinds { get; }

        public IDiagnosticLocation DiagnosticLocation => this.Syntax.DiagnosticLocation;
        
        public SyntaxReference Syntax { get; }

        internal ValidateReferenceContext(
            IDeclaration referencedDeclaration,
            IDeclaration referencingDeclaration,
            in SyntaxReference syntax,
            IAspectState? aspectState,
            IDiagnosticSink diagnostics,
            ValidatedReferenceKinds referenceKinds )
        {
            this.AspectState = aspectState;
            this.Diagnostics = diagnostics;
            this.ReferencedDeclaration = referencedDeclaration;
            this.ReferencingDeclaration = referencingDeclaration;
            this.ReferenceKinds = referenceKinds;
            this.Syntax = syntax;
        }
    }
}