// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            this.MemberBuilder.Type = this.TemplateMember?.Type ?? this.TargetDeclaration.Compilation.TypeFactory.GetSpecialType( SpecialType.Object );
            this.MemberBuilder.Accessibility = this.TemplateMember?.Accessibility ?? Accessibility.Private;
            this.MemberBuilder.IsStatic = this.TemplateMember?.IsStatic ?? false;

            if ( this.TemplateMember != null )
            {
                var declarator = (VariableDeclaratorSyntax) this.TemplateMember.GetPrimaryDeclaration().AssertNotNull();

                if ( declarator.Initializer != null )
                {
                    this.MemberBuilder.HasInitializerTemplate = true;
                }
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create( this.MemberBuilder );
        }
    }
}