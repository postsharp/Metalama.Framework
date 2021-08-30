// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<IEvent, EventBuilder>
    {
        private readonly Template<IMethod> _addTemplate;
        private readonly Template<IMethod> _removeTemplate;

        // ReSharper disable once MemberCanBePrivate.Global

        public IEventBuilder Builder => this.MemberBuilder;

        public IntroduceEventAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string? explicitName,
            Template<IEvent> eventTemplate,
            Template<IMethod> addTemplate,
            Template<IMethod> removeTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, eventTemplate, scope, overrideStrategy, layerName, tags )
        {
            this._addTemplate = addTemplate;
            this._removeTemplate = removeTemplate;

            this.MemberBuilder = new EventBuilder(
                this,
                this.TargetDeclaration,
                eventTemplate.Declaration?.Name ?? explicitName.AssertNotNull(),
                eventTemplate.Declaration != null && eventTemplate.Declaration.IsEventField() );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            // TODO: Checks.

            this.MemberBuilder.EventType =
                (this.TemplateMember?.EventType ?? (INamedType?) this._addTemplate.Declaration?.Parameters.FirstOrDefault().AssertNotNull().ParameterType)
                .AssertNotNull();

            this.MemberBuilder.Accessibility = (this.TemplateMember?.Accessibility ?? this._addTemplate.Declaration?.Accessibility).AssertNotNull();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Override transformations.

            if ( this.TemplateMember != null && this.TemplateMember.IsEventField() )
            {
                return AdviceResult.Create( this.MemberBuilder );
            }
            else
            {
                return AdviceResult.Create(
                    this.MemberBuilder,
                    new OverriddenEvent(
                        this,
                        this.MemberBuilder,
                        this.Template,
                        this._addTemplate,
                        this._removeTemplate ) );
            }
        }
    }
}