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
    public readonly struct ValidateReferenceContext<T>
        where T : IDeclaration
    {
        public IAspectState? AspectState { get; }
        
        public IDiagnosticSink Diagnostics { get; }

        public T ReferencedDeclaration { get; }

        public IDeclaration ReferencingDeclaration { get; }

        public INamedType ReferencingType { get; }

        public ValidatedReferenceKinds ReferenceKinds { get; }

        // Must be a lazy implementation.
        public IDiagnosticLocation DiagnosticLocation { get; }
    }
}