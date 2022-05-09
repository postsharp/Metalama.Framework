// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
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

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public IntroduceFieldAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IField> fieldTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            ITagReader tags )
            : base( aspect, templateInstance, targetDeclaration, fieldTemplate, scope, overrideStrategy, layerName, tags )
        {
            this.MemberBuilder = new FieldBuilder( this, targetDeclaration, (explicitName ?? fieldTemplate.Declaration?.Name).AssertNotNull(), tags );
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
                this.MemberBuilder.Type = this.SourceCompilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Object );
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