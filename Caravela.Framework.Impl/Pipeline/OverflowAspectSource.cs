// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An <see cref="IAspectSource"/> that stores aspect sources that are not a part of the current <see cref="PipelineStage"/>.
    /// </summary>
    internal class OverflowAspectSource : IAspectSource
    {
        private readonly List<(IAspectSource Source, AspectClass AspectClass)> _aspectSources = new();

        public AspectSourcePriority Priority => AspectSourcePriority.Aggregate;

        public IEnumerable<AspectClass> AspectTypes => this._aspectSources.Select( a => a.AspectClass ).Distinct();

        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            AspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            var aspectTypeSymbol = compilation.RoslynCompilation.GetTypeByMetadataName( aspectClass.FullName ).AssertNotNull();

            return this._aspectSources
                .Where( s => s.AspectClass.FullName.Equals( aspectTypeSymbol.GetReflectionName(), StringComparison.Ordinal ) )
                .Select( a => a.Source )
                .Distinct()
                .SelectMany( a => a.GetAspectInstances( compilation, aspectClass, diagnosticAdder, cancellationToken ) );
        }

        public void Add( IAspectSource aspectSource, AspectClass aspectClass )
        {
            this._aspectSources.Add( (aspectSource, aspectClass) );
        }
    }
}