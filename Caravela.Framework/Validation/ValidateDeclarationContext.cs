// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Validation
{
    public readonly struct ValidateDeclarationContext<T>
    {
        private IDiagnosticSink Diagnostics { get; }

        private T Declaration { get; }
    }
}