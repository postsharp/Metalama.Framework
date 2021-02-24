// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// The location to which a diagnostic can be emitted in user code. This interface has no member that are useful in user code.
    /// </summary>
    public interface IDiagnosticLocation : IEquatable<IDiagnosticLocation?>
    {
    }
}