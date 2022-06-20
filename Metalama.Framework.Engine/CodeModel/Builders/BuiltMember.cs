// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class BuiltMember : BuiltMemberOrNamedType, IMemberImpl
    {
        protected BuiltMember( CompilationModel compilation, MemberBuilder builder ) : base( compilation, builder ) { }

        public abstract MemberBuilder MemberBuilder { get; }

        public sealed override DeclarationBuilder Builder => this.MemberBuilder;

        public bool IsExplicitInterfaceImplementation => this.MemberBuilder.IsExplicitInterfaceImplementation;

        public bool IsImplicit => this.MemberBuilder.IsImplicit;

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public bool IsVirtual => this.MemberBuilder.IsVirtual;

        public bool IsAsync => this.MemberBuilder.IsAsync;

        public bool IsOverride => this.MemberBuilder.IsOverride;

        public IMember? OverriddenMember => this.Compilation.Factory.GetDeclaration( this.MemberBuilder.OverriddenMember );
    }
}