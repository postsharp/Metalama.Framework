// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroducePropertyAdvice : IntroduceMemberAdvice<PropertyBuilder>, IIntroducePropertyAdvice
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
            AspectLinkerOptions? linkerOptions,
            IReadOnlyDictionary<string, object?> tags )
            : base( aspect, targetDeclaration, templateProperty, scope, conflictBehavior, linkerOptions, tags )
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
                this.TemplateMember != null && IsAutoProperty( this.TemplateMember ),
                this.TemplateMember != null && HasInitOnlySetter( this.TemplateMember ),
                linkerOptions );
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
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

        private static bool HasInitOnlySetter( IProperty templateProperty )
        {
            var symbol = (IPropertySymbol) templateProperty.GetSymbol().AssertNotNull();

            return symbol.SetMethod?.IsInitOnly == true;
        }

        private static bool IsAutoProperty( IProperty templateProperty )
        {
            var symbol = (IPropertySymbol) templateProperty.GetSymbol().AssertNotNull();
            var syntax = symbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax(); // TODO: Partial?

            if ( syntax == null )
            {
                // TODO: How to detect without source code?
                return false;
            }

            return ((PropertyDeclarationSyntax) syntax).AccessorList?.Accessors.All( a => a.Body == null && a.ExpressionBody == null ) == true;
        }
    }
}