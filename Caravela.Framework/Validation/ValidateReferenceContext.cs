// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Validation
{
    public readonly struct ValidateReferenceContext<T>
        where T : IDeclaration
    {
        public IDiagnosticSink Diagnostics { get; }

        public T ReferencedDeclaration { get; }

        public IDeclaration ReferencingDeclaration { get; }

        public INamedType ReferencingType { get; }

        public DeclarationReferenceKind ReferenceKind { get; }

        // Must be a lazy implementation.
        public IDiagnosticLocation DiagnosticLocation { get; }
    }
}