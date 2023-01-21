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
    // TODO: Check why this class is unused.
    // ReSharper disable once UnusedType.Global

    internal class OverrideFinalizerAdvice : OverrideMemberAdvice<IMethod>
    {
        private readonly BoundTemplateMethod _boundTemplate;

        public OverrideFinalizerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMethod targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this._boundTemplate = boundTemplate;
        }

        public override AdviceKind AdviceKind => AdviceKind.OverrideFinalizer;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            addTransformation( new OverrideFinalizerTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

            return AdviceImplementationResult.Success();
        }
    }
}