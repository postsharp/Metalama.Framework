// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
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

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
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