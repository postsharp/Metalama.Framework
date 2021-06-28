// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.Advices;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilder
    {
        protected MemberBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice, declaringType, name ) { }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public override string ToString() => this.DeclaringType + "." + this.Name;

        public abstract bool IsExplicitInterfaceImplementation { get; }
    }
}