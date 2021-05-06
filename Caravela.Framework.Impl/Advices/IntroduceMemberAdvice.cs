// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class IntroduceMemberAdvice : Advice
    {
        public IntroductionScope Scope { get; }

        public ConflictBehavior ConflictBehavior { get; }

        // Null is for types.
        public new INamedType? TargetDeclaration => (INamedType) base.TargetDeclaration;

        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroduceMemberAdvice( AspectInstance aspect, INamedType? targetDeclaration, IntroductionScope scope, ConflictBehavior conflictBehavior, AspectLinkerOptions? linkerOptions ) : base( aspect, targetDeclaration )
        {
            this.Scope = scope;
            this.ConflictBehavior = conflictBehavior;
            this.LinkerOptions = linkerOptions;
        }
    }
}
