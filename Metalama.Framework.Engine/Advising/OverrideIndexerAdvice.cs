// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal class OverrideIndexerAdvice : OverrideMemberAdvice<IIndexer>
    {
        public TemplateMember<IProperty>? PropertyTemplate { get; }

        public BoundTemplateMethod? GetTemplate { get; }

        public BoundTemplateMethod? SetTemplate { get; }

        public OverrideIndexerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IIndexer targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
        {
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
        }

        public override AdviceKind AdviceKind => AdviceKind.OverrideFieldOrPropertyOrIndexer;

        public override AdviceImplementationResult Implement(
            IServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            addTransformation( new OverrideIndexerTransformation( this, targetDeclaration, this.GetTemplate, this.SetTemplate, this.Tags ) );

            return AdviceImplementationResult.Success();
        }
    }
}