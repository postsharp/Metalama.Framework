// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class BuiltMember : BuiltMemberOrNamedType, IMember, IMemberRef<IMember>
    {
        protected BuiltMember( CompilationModel compilation ) : base( compilation ) { }

        IMember IDeclarationRef<IMember>.Resolve( CompilationModel compilation ) => throw new NotImplementedException();

        ISymbol IDeclarationRef<IMember>.GetSymbol( Compilation compilation ) => throw new NotImplementedException();

        public bool IsExplicitInterfaceImplementation => ((MemberBuilder) this.Builder).IsExplicitInterfaceImplementation;

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();
        
        public bool IsVirtual => ((MemberBuilder) this.MemberOrNamedTypeBuilder).IsVirtual;

        public bool IsAsync => ((MemberBuilder) this.MemberOrNamedTypeBuilder).IsAsync;


    }
}