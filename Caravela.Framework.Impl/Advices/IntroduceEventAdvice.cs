// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceEventAdvice : IntroduceMemberAdvice<EventBuilder>
    {
        private readonly IMethod? _addTemplateMethod;
        private readonly IMethod? _removeTemplateMethod;

        public new IEvent? TemplateMember => (IEvent?) base.TemplateMember;

        public IEventBuilder Builder => this.MemberBuilder;

        public IntroduceEventAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IEvent? eventTemplate,
            string? explicitName,
            IMethod? addTemplateMethod,
            IMethod? removeTemplateMethod,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            string? layerName,
            AdviceOptions? options )
            : base( aspect, targetDeclaration, eventTemplate, scope, conflictBehavior, layerName, options )
        {
            this._addTemplateMethod = addTemplateMethod;
            this._removeTemplateMethod = removeTemplateMethod;

            this.MemberBuilder = new EventBuilder(
                this,
                this.TargetDeclaration,
                eventTemplate?.Name ?? explicitName.AssertNotNull(),
                eventTemplate != null && IsEventField( eventTemplate ),
                options?.LinkerOptions );
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );

            // TODO: Checks.

            this.MemberBuilder.EventType =
                (this.TemplateMember?.EventType ?? this._addTemplateMethod?.Parameters.FirstOrDefault().AssertNotNull().ParameterType).AssertNotNull();

            this.MemberBuilder.Accessibility = (this.TemplateMember?.Accessibility ?? this._addTemplateMethod?.Accessibility).AssertNotNull();
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Override transformations.

            return AdviceResult.Create( this.MemberBuilder );
        }

        private static bool IsEventField( IEvent templateEvent )
        {
            var symbol = (IEventSymbol) templateEvent.GetSymbol().AssertNotNull();
            var syntax = symbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax(); // TODO: Partial?

            if ( syntax == null )
            {
                // TODO: How to detect without source code?
                return false;
            }

            return syntax is VariableDeclaratorSyntax;
        }
    }
}