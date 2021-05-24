// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class BuiltMemberOrNamedType : BuiltDeclaration, IMemberOrNamedType, IMemberRef<IMemberOrNamedType>
    {
        protected BuiltMemberOrNamedType( CompilationModel compilation ) : base( compilation ) { }

        public abstract MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder { get; }

        public Accessibility Accessibility => this.MemberOrNamedTypeBuilder.Accessibility;

        public string Name => this.MemberOrNamedTypeBuilder.Name;

        public bool IsAbstract => this.MemberOrNamedTypeBuilder.IsAbstract;

        public bool IsStatic => this.MemberOrNamedTypeBuilder.IsStatic;

        public bool IsVirtual => this.MemberOrNamedTypeBuilder.IsVirtual;

        public bool IsSealed => this.MemberOrNamedTypeBuilder.IsSealed;

        public bool IsReadOnly => this.MemberOrNamedTypeBuilder.IsReadOnly;

        public bool IsOverride => this.MemberOrNamedTypeBuilder.IsOverride;

        public bool IsNew => this.MemberOrNamedTypeBuilder.IsNew;

        public bool IsAsync => this.MemberOrNamedTypeBuilder.IsAsync;

        public INamedType? DeclaringType => this.Compilation.Factory.GetDeclaration( this.MemberOrNamedTypeBuilder.DeclaringType );

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        IMemberOrNamedType IDeclarationRef<IMemberOrNamedType>.Resolve( CompilationModel compilation )
            => (IMemberOrNamedType) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IMemberOrNamedType>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public object? Target => throw new NotImplementedException();
    }
}