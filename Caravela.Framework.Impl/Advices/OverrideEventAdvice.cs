// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideEventAdvice : OverrideMemberAdvice<IEvent>
    {
        public Template<IEvent> EventTemplate { get; }

        public Template<IMethod> AddTemplate { get; }

        public Template<IMethod> RemoveTemplate { get; }

        public OverrideEventAdvice(
            AspectInstance aspect,
            IEvent targetDeclaration,
            Template<IEvent> eventTemplate,
            Template<IMethod> addTemplate,
            Template<IMethod> removeTemplate,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, layerName, tags )
        {
            // We need either property template or both accessor templates, but never both.
            Invariant.Assert( eventTemplate.IsNotNull || (addTemplate.IsNotNull && removeTemplate.IsNotNull) );
            Invariant.Assert( !(eventTemplate.IsNotNull && (addTemplate.IsNotNull || removeTemplate.IsNotNull)) );

            this.EventTemplate = eventTemplate;
            this.AddTemplate = addTemplate;
            this.RemoveTemplate = removeTemplate;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create(
                new OverriddenEvent(
                    this,
                    this.TargetDeclaration,
                    this.EventTemplate,
                    this.AddTemplate,
                    this.RemoveTemplate ) );
        }
    }
}