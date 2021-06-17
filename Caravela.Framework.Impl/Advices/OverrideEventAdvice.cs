﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideEventAdvice : Advice
    {
        public IEvent? TemplateEvent { get; }

        public IMethod? AddTemplateMethod { get; }

        public IMethod? RemoveTemplateMethod { get; }

        public new IEvent TargetDeclaration => (IEvent) base.TargetDeclaration;

        public OverrideEventAdvice(
            AspectInstance aspect,
            IEvent targetDeclaration,
            IEvent? templateEvent,
            IMethod? addTemplateMethod,
            IMethod? removeTemplateMethod,
            string layerName,
            AdviceOptions? options )
            : base( aspect, targetDeclaration, layerName, options )
        {
            // We need either property template or both accessor templates, but never both.
            Invariant.Assert( templateEvent != null || (addTemplateMethod != null && removeTemplateMethod != null) );
            Invariant.Assert( !(templateEvent != null && (addTemplateMethod != null || removeTemplateMethod != null)) );

            this.TemplateEvent = templateEvent;
            this.AddTemplateMethod = addTemplateMethod;
            this.RemoveTemplateMethod = removeTemplateMethod;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create(
                new OverriddenEvent( this, this.TargetDeclaration, this.TemplateEvent, this.AddTemplateMethod, this.RemoveTemplateMethod, this.LinkerOptions ) );
        }
    }
}