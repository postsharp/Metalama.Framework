// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Impl.Advices;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Utilities;

namespace Metalama.Framework.Impl.Aspects
{
    /// <summary>
    /// Defines the internal semantics of classes implementing <see cref="IAspectBuilder"/>. This interface
    /// exists because the only implementation <see cref="AspectBuilder{T}"/> is generic, and some parts of the
    /// code need a common, non-generic interface.
    /// </summary>
    internal interface IAspectBuilderInternal : IAspectBuilder
    {
        void AddAspectSource( IAspectSource aspectSource );

        AdviceFactory AdviceFactory { get; }

        DisposeAction WithPredecessor( in AspectPredecessor predecessor );

        IDiagnosticAdder DiagnosticAdder { get; }
    }
}