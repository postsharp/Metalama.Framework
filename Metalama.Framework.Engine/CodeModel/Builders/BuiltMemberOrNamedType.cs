// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class BuiltMemberOrNamedType : BuiltDeclaration, IMemberOrNamedType, IMemberRef<IMemberOrNamedType>
    {
        protected BuiltMemberOrNamedType( CompilationModel compilation ) : base( compilation ) { }

        public abstract MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder { get; }

        public Accessibility Accessibility => this.MemberOrNamedTypeBuilder.Accessibility;

        public string Name => this.MemberOrNamedTypeBuilder.Name;

        public bool IsAbstract => this.MemberOrNamedTypeBuilder.IsAbstract;

        public bool IsStatic => this.MemberOrNamedTypeBuilder.IsStatic;

        public bool IsSealed => this.MemberOrNamedTypeBuilder.IsSealed;

        public bool IsNew => this.MemberOrNamedTypeBuilder.IsNew;

        public INamedType? DeclaringType => this.Compilation.Factory.GetDeclaration( this.MemberOrNamedTypeBuilder.DeclaringType );

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        string? IRef<IMemberOrNamedType>.ToSerializableId() => null;

        IMemberOrNamedType IRef<IMemberOrNamedType>.GetTarget( ICompilation compilation ) => (IMemberOrNamedType) this.GetForCompilation( compilation );

        ISymbol? ISdkRef<IMemberOrNamedType>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();

        public object? Target => throw new NotImplementedException();
    }
}