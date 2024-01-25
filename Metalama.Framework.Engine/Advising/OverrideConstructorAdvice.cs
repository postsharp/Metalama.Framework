// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    internal class OverrideConstructorAdvice : OverrideMemberAdvice<IConstructor>
    {
        private readonly BoundTemplateMethod _boundTemplate;

        public OverrideConstructorAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IConstructor targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this._boundTemplate = boundTemplate;
        }

        public override AdviceKind AdviceKind => AdviceKind.OverrideConstructor;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            addTransformation( new OverrideConstructorTransformation( this, this.TargetDeclaration.GetTarget( compilation ), this._boundTemplate, this.Tags ) );

            return AdviceImplementationResult.Success();
        }
    }
}