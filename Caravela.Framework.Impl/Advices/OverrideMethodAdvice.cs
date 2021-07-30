// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class OverrideMethodAdvice : OverrideMemberAdvice<IMethod>
    {
        public Template<IMethod> Template { get; }

        public OverrideMethodAdvice(
            AspectInstance aspect,
            IMethod targetDeclaration,
            Template<IMethod> template,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, targetDeclaration, layerName, tags )
        {
            this.Template = template;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create( new OverriddenMethod( this, this.TargetDeclaration, this.Template ) );
        }
    }
}