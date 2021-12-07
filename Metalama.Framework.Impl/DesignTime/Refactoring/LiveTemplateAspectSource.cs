// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Refactoring
{
    /// <summary>
    /// A fake instance of <see cref="IAspectSource"/> to avoid having to support null sources.
    /// </summary>
    internal sealed class LiveTemplateAspectSource : IAspectSource
    {
        public static readonly LiveTemplateAspectSource Instance = new();

        private LiveTemplateAspectSource() { }

        public ImmutableArray<IAspectClass> AspectClasses => ImmutableArray<IAspectClass>.Empty;

        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Array.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
            => Array.Empty<AspectInstance>();
    }
}