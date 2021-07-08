// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal class IntroduceFieldAdvice : IntroduceMemberAdvice<FieldBuilder>
    {
        public IFieldBuilder Builder => this.MemberBuilder;

        public new INamedType TargetDeclaration => base.TargetDeclaration!;

        public IntroduceFieldAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            string name,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string layerName,
            Dictionary<string, object?>? tags )
            : base( aspect, targetDeclaration, null, scope, overrideStrategy, layerName, tags )
        {
            this.MemberBuilder = new FieldBuilder( this, this.TargetDeclaration, name, AspectLinkerOptions.FromTags( tags ) );
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( declarativeAdvices, diagnosticAdder );
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            throw new NotImplementedException();
        }
    }
}