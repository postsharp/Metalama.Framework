// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [Obsolete( "Not implemented." )]
    [CompileTimeOnly]
    public readonly struct ValidateDeclarationContext<T>
        where T : IDeclaration
    {
        public IDiagnosticSink Diagnostics { get; }

        public T Declaration { get; }
    }
}