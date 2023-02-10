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
    internal sealed class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrProperty>
    {
        private readonly BoundTemplateMethod? _getTemplate;
        private readonly BoundTemplateMethod? _setTemplate;

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrProperty targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this._getTemplate = getTemplate.ExplicitlyImplementedOrNull();
            this._setTemplate = setTemplate.ExplicitlyImplementedOrNull();
        }

        public override AdviceKind AdviceKind => AdviceKind.OverrideFieldOrPropertyOrIndexer;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // TODO: Translate templates to this compilation.
            // TODO: order should be self if the target is introduced on the same layer.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var promotedField = OverrideHelper.OverrideProperty(
                serviceProvider,
                this,
                targetDeclaration,
                this._getTemplate,
                this._setTemplate,
                this.Tags,
                addTransformation );

            return AdviceImplementationResult.Success( promotedField );
        }
    }
}