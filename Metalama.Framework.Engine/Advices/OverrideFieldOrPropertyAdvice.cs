// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrPropertyOrIndexer>
    {
        private readonly IObjectReader? _args;

        public TemplateMember<IProperty> PropertyTemplate { get; }

        public TemplateMember<IMethod> GetTemplate { get; }

        public TemplateMember<IMethod> SetTemplate { get; }

        public OverrideFieldOrPropertyAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IFieldOrPropertyOrIndexer targetDeclaration,
            TemplateMember<IProperty> propertyTemplate,
            TemplateMember<IMethod> getTemplate,
            TemplateMember<IMethod> setTemplate,
            string? layerName,
            IObjectReader tags,
            IObjectReader? args )
            : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.PropertyTemplate = propertyTemplate;
            this.GetTemplate = getTemplate;
            this.SetTemplate = setTemplate;
            this._args = args;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            // TODO: Translate templates to this compilation.
            // TODO: order should be self if the target is introduced on the same layer.
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            return AdviceResult.Create(
                OverrideHelper.OverrideProperty(
                    this,
                    targetDeclaration,
                    this.PropertyTemplate,
                    this.GetTemplate,
                    this.SetTemplate,
                    ForOverride,
                    this.Tags ) );

            BoundTemplateMethod ForOverride(TemplateMember<IMethod> templateMember, IMethod? targetMethod )
            {
                return templateMember.ForOverride( targetMethod, this._args );
            }
        }
    }
}