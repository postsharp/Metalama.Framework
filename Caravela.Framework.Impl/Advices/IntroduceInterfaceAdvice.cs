// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceInterfaceAdvice : Advice, IIntroduceInterfaceAdvice
    {
        public INamedType InterfaceType { get; }

        public bool IsExplicit { get; }

        public IReadOnlyDictionary<IMember, IMember>? MemberMap { get; }

        public ConflictBehavior ConflictBehavior { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public IntroduceInterfaceAdvice(
            AspectInstance aspect,
            INamedType targetType,
            INamedType interfaceType,
            bool isExplicit,
            IReadOnlyDictionary<IMember, IMember>? memberMap,
            ConflictBehavior conflictBehavior,
            AspectLinkerOptions? linkerOptions,
            IReadOnlyDictionary<string, object?> aspectTags ) : base(aspect, targetType, aspectTags)
        {
            this.InterfaceType = interfaceType;
            this.IsExplicit = isExplicit;
            this.MemberMap = memberMap;
            this.ConflictBehavior = conflictBehavior;
            this.LinkerOptions = linkerOptions;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            return AdviceResult.Create( new IntroducedInterface( this, this.TargetDeclaration, this.InterfaceType, this.IsExplicit, this.MemberMap ) );
        }
    }
}
