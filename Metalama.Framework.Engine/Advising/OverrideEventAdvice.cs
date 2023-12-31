﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class OverrideEventAdvice : OverrideMemberAdvice<IEvent>
    {
        private readonly BoundTemplateMethod? _addTemplate;
        private readonly BoundTemplateMethod? _removeTemplate;

        public OverrideEventAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IEvent targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod? addTemplate,
            BoundTemplateMethod? removeTemplate,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            Invariant.Assert( addTemplate != null || removeTemplate != null );

            this._addTemplate = addTemplate;
            this._removeTemplate = removeTemplate;
        }

        public override AdviceKind AdviceKind => AdviceKind.OverrideEvent;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            addTransformation(
                new OverrideEventTransformation(
                    this,
                    this.TargetDeclaration.GetTarget( compilation ),
                    this._addTemplate,
                    this._removeTemplate,
                    this.Tags ) );

            return AdviceImplementationResult.Success();
        }
    }
}