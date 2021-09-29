// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IAspectSource"/> that relies on a delegate that returns the aspect instances.
    /// </summary>
    internal class ProgrammaticAspectSource<TAspect, TDeclaration> : IAspectSource
        where TDeclaration : class, IDeclaration
        where TAspect : IAspect<TDeclaration>
    {
        private readonly Func<CompilationModel, IEnumerable<AspectInstance>> _getInstances;
        private readonly IAspectClass _aspectClass;

        public ProgrammaticAspectSource( IAspectClass aspectClass, Func<CompilationModel, IEnumerable<AspectInstance>> getInstances )
        {
            if ( aspectClass.FullName != typeof(TAspect).FullName )
            {
                throw new ArgumentOutOfRangeException( nameof(aspectClass) );
            }

            this._getInstances = getInstances;
            this._aspectClass = aspectClass;
        }

        public AspectSourcePriority Priority => AspectSourcePriority.Programmatic;

        public IEnumerable<IAspectClass> AspectClasses => new[] { this._aspectClass };

        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Array.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            return this._getInstances( compilation );
        }
    }
}