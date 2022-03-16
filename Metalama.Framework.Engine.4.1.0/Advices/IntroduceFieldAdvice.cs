// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, FieldBuilder>
    {
        public IFieldBuilder Builder => this.MemberBuilder;

        public new INamedType TargetDeclaration => base.TargetDeclaration;

        public IntroduceFieldAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IField> fieldTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName )
            : base( aspect, templateInstance, targetDeclaration, fieldTemplate, scope, overrideStrategy, layerName, null )
        {
            this.MemberBuilder = new FieldBuilder( this, this.TargetDeclaration, (explicitName ?? fieldTemplate.Declaration?.Name).AssertNotNull() );
            this.MemberBuilder.InitializerTemplate = fieldTemplate.GetInitializerTemplate();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            if ( !this.Template.IsNull )
            {
                this.MemberBuilder.Type = this.Template.Declaration!.Type;
                this.MemberBuilder.Accessibility = this.Template.Declaration!.Accessibility;
                this.MemberBuilder.IsStatic = this.Template.Declaration!.IsStatic;
            }
            else
            {
                this.MemberBuilder.Type = this.TargetDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Object );
                this.MemberBuilder.Accessibility = Accessibility.Private;
                this.MemberBuilder.IsStatic = false;
            }
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            return AdviceResult.Create( this.MemberBuilder );
        }
    }
}