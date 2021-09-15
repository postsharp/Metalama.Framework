// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Wraps a Roslyn <see cref="Location"/> into a <see cref="DiagnosticLocation"/>.
    /// </summary>
    internal class DiagnosticLocation : IDiagnosticLocation
    {
        public DiagnosticLocation( Location? location )
        {
            this.Location = location;
        }

        public Location? Location { get; }
    }
}