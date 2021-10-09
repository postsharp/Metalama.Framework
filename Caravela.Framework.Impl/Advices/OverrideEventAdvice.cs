// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideEventAdvice : OverrideMemberAdvice<IEvent>
    {
        public TemplateMember<IEvent> EventTemplate { get; }

        public TemplateMember<IMethod> AddTemplate { get; }

        public TemplateMember<IMethod> RemoveTemplate { get; }

        public OverrideEventAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IEvent targetDeclaration,
            TemplateMember<IEvent> eventTemplate,
            TemplateMember<IMethod> addTemplate,
            TemplateMember<IMethod> removeTemplate,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, templateInstance, targetDeclaration, layerName, tags )
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
            // TODO: order should be self if the target is introduced on the same layer.
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