// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice
    {
        public AspectInstance Aspect { get; }

        public IDeclaration TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        public AdviceOptions Options { get; }

        public AspectLinkerOptions? LinkerOptions => this.Options.LinkerOptions;

        protected Advice( AspectInstance aspect, IDeclaration targetDeclaration, string? layerName, AdviceOptions? options )
        {
            this.Options = options ?? AdviceOptions.Default;
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration.AssertNotNull();
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, layerName );
        }

        public abstract void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder );

        public abstract AdviceResult ToResult( ICompilation compilation );
    }
}