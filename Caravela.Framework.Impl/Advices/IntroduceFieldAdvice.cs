// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, FieldBuilder>
    {
        public IFieldBuilder Builder => this.MemberBuilder;

        public new INamedType TargetDeclaration => base.TargetDeclaration;

        public IntroduceFieldAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string? explicitName,
            Template<IField> fieldTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName )
            : base( aspect, targetDeclaration, fieldTemplate, scope, overrideStrategy, layerName, null )
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
                this.MemberBuilder.InitializerSyntax = declarator.Initializer?.Value;
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create( this.MemberBuilder );
        }
    }
}