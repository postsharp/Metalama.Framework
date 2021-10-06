// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Defines the semantics of an object that can return a set of <see cref="AspectInstance"/>
    /// for a given <see cref="IAspectClass"/>.
    /// </summary>
    internal interface IAspectSource
    {
        AspectSourcePriority Priority { get; }

        ImmutableArray<IAspectClass> AspectClasses { get; }

        IEnumerable<IDeclaration> GetExclusions( INamedType aspectType );

        /// <summary>
        /// Returns a set of <see cref="AspectInstance"/> of a given type. This method is called when the given aspect
        /// type is being processed, not before.
        /// </summary>
        IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken );
    }
}