// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class Advice
    {
        public IAspectInstanceInternal Aspect { get; }

        public TemplateClassInstance TemplateInstance { get; }

        public IDeclaration TargetDeclaration { get; }

        public AspectLayerId AspectLayerId { get; }

        public Dictionary<string, object?>? Tags { get; }

        public ImmutableDictionary<string, object?> ReadOnlyTags => this.Tags?.ToImmutableDictionary() ?? ImmutableDictionary<string, object?>.Empty;

        public int Order { get; set; }

        protected Advice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance template,
            IDeclaration targetDeclaration,
            string? layerName,
            Dictionary<string, object?>? tags )
        {
            this.Tags = tags;
            this.Aspect = aspect;
            this.TemplateInstance = template;
            this.TargetDeclaration = targetDeclaration.AssertNotNull();
            this.AspectLayerId = new AspectLayerId( this.Aspect.AspectClass, layerName );
        }

        public abstract void Initialize( IDiagnosticAdder diagnosticAdder );

        public abstract AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations );
    }
}