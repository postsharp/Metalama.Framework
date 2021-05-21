// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice : IAdvice
    {
        public AspectInstance Aspect { get; }

        IAspect IAdvice.Aspect => this.Aspect.Aspect;

        public IDeclaration TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        public string? LayerName { get; set; }

        /// <summary>
        /// Gets a read-only of tags set by <see cref="IAspectBuilder.Tags"/>.
        /// </summary>
        public IReadOnlyDictionary<string, object?> AspectBuilderTags { get; }

        protected Advice( AspectInstance aspect, IDeclaration targetDeclaration, IReadOnlyDictionary<string, object?> aspectTags )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration.AssertNotNull();
            this.AspectBuilderTags = aspectTags;
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, this.LayerName );
        }

        public abstract void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder );

        public abstract AdviceResult ToResult( ICompilation compilation );
    }
}