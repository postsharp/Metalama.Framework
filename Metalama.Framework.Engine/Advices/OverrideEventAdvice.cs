// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideEventAdvice : OverrideMemberAdvice<IEvent>
    {
        private readonly IObjectReader _parameters;

        public TemplateMember<IEvent> EventTemplate { get; }

        public TemplateMember<IMethod> AddTemplate { get; }

        public TemplateMember<IMethod> RemoveTemplate { get; }

        public OverrideEventAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IEvent targetDeclaration,
            ICompilation sourceCompilation,
            TemplateMember<IEvent> eventTemplate,
            TemplateMember<IMethod> addTemplate,
            TemplateMember<IMethod> removeTemplate,
            string? layerName,
            IObjectReader tags,
            IObjectReader parameters )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this._parameters = parameters;

            // We need either property template or both accessor templates, but never both.
            Invariant.Assert( eventTemplate.IsNotNull || (addTemplate.IsNotNull && removeTemplate.IsNotNull) );
            Invariant.Assert( !(eventTemplate.IsNotNull && (addTemplate.IsNotNull || removeTemplate.IsNotNull)) );

            this.EventTemplate = eventTemplate;
            this.AddTemplate = addTemplate;
            this.RemoveTemplate = removeTemplate;
        }

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            addTransformation(
                new OverrideEventTransformation(
                    this,
                    this.TargetDeclaration.GetTarget( compilation ),
                    this.EventTemplate,
                    this.AddTemplate,
                    this.RemoveTemplate,
                    this.Tags,
                    this._parameters ) );

            return AdviceImplementationResult.Success();
        }
    }
}