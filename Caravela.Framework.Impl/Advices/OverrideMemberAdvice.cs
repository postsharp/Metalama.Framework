// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class OverrideMemberAdvice<TMember> : Advice
        where TMember : class, IMember
    {
        public new TMember TargetDeclaration => (TMember) base.TargetDeclaration;

        public OverrideMemberAdvice(
            AspectInstance aspect,
            TMember targetDeclaration,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, targetDeclaration, layerName, tags ) { }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            // TODO: Test that the advice is not applied to declaration in a base class.
        }
    }
}