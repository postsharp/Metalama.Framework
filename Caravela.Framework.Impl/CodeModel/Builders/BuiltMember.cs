// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class BuiltMember : BuiltMemberOrNamedType, IMemberImpl, IMemberRef<IMember>
    {
        protected BuiltMember( CompilationModel compilation ) : base( compilation ) { }

        public abstract MemberBuilder MemberBuilder { get; }

        public sealed override DeclarationBuilder Builder => this.MemberBuilder;

        IMember IRef<IMember>.GetTarget( ICompilation compilation ) => throw new NotImplementedException();

        ISymbol ISdkRef<IMember>.GetSymbol( Compilation compilation ) => throw new NotImplementedException();

        public bool IsExplicitInterfaceImplementation => this.MemberBuilder.IsExplicitInterfaceImplementation;

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public bool IsVirtual => this.MemberBuilder.IsVirtual;

        public bool IsAsync => this.MemberBuilder.IsAsync;

        public bool IsOverride => this.MemberBuilder.IsOverride;

        public IMember? OverriddenMember => this.Compilation.Factory.GetDeclaration( this.MemberBuilder.OverriddenMember );
    }
}