// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using System;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Caravela.Framework.Validation
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [Obsolete( "Not implemented." )]
    public readonly struct ValidateDeclarationContext<T>
    {
        public IDiagnosticSink Diagnostics { get; }

        public T Declaration { get; }
    }
}