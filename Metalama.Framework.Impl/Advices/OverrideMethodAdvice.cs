﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.Advices
{
    internal class OverrideMethodAdvice : OverrideMemberAdvice<IMethod>
    {
        public TemplateMember<IMethod> Template { get; }

        public OverrideMethodAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMethod targetDeclaration,
            TemplateMember<IMethod> template,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.Template = template;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: order should be self if the target is introduced on the same layer.
            return AdviceResult.Create( new OverriddenMethod( this, this.TargetDeclaration, this.Template ) );
        }
    }
}