// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<PropertyBuilder>
    {
        private readonly IMethod? _getTemplateMethod;
        private readonly IMethod? _setTemplateMethod;

        public new IProperty? TemplateMember => (IProperty?) base.TemplateMember;

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IPropertyBuilder Builder => this.MemberBuilder;

        public IntroducePropertyAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IProperty? templateProperty,
            string? explicitName,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            string? layerName,
            AdviceOptions? options )
            : base( aspect, targetDeclaration, templateProperty, scope, conflictBehavior, layerName, options )
        {
            this._getTemplateMethod = getTemplateMethod;
            this._setTemplateMethod = setTemplateMethod;

            var name = templateProperty?.Name ?? explicitName ?? throw new AssertionFailedException();
            var hasGet = templateProperty != null ? templateProperty.Getter != null : getTemplateMethod != null;
            var hasSet = templateProperty != null ? templateProperty.Setter != null : setTemplateMethod != null;

            this.MemberBuilder = new PropertyBuilder(
                this,
                this.TargetDeclaration,
                name,
                hasGet,
                hasSet,
                this.TemplateMember != null && this.TemplateMember.IsAutoPropertyOrField,
                this.TemplateMember != null && this.TemplateMember.Writeability == Writeability.InitOnly,
                options?.LinkerOptions );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            // TODO: The rest (unify with methods?).

            this.MemberBuilder.Type = (this.TemplateMember?.Type ?? this._getTemplateMethod?.ReturnType).AssertNotNull();
            this.MemberBuilder.Accessibility = (this.TemplateMember?.Accessibility ?? this._getTemplateMethod?.Accessibility).AssertNotNull();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create(
                this.MemberBuilder,
                new OverriddenProperty( this, this.MemberBuilder, this.TemplateMember, this._getTemplateMethod, this._setTemplateMethod, this.LinkerOptions ) );
        }
    }
}