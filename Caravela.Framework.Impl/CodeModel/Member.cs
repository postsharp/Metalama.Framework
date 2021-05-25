// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Member : MemberOrNamedType, IMember
    {
        protected Member( CompilationModel compilation ) : base( compilation ) { }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public abstract bool IsAsync { get; }

        public bool IsVirtual => this.Symbol.IsVirtual;

        public bool IsOverride => this.Symbol.IsOverride;
    }
}