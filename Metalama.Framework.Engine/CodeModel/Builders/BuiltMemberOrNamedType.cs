// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltMemberOrNamedType : BuiltNamedDeclaration, IMemberOrNamedTypeImpl
{
    protected BuiltMemberOrNamedType( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder { get; }

    public Accessibility Accessibility => this.MemberOrNamedTypeBuilder.Accessibility;

    public bool IsAbstract => this.MemberOrNamedTypeBuilder.IsAbstract;

    public bool IsStatic => this.MemberOrNamedTypeBuilder.IsStatic;

    public bool IsSealed => this.MemberOrNamedTypeBuilder.IsSealed;

    public bool IsNew => this.MemberOrNamedTypeBuilder.IsNew;

    public bool? HasNewKeyword => this.MemberOrNamedTypeBuilder.HasNewKeyword;

    public INamedType? DeclaringType
        => this.Compilation.Factory.Translate( this.MemberOrNamedTypeBuilder.DeclaringType, ReferenceResolutionOptions.CanBeMissing );

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    ExecutionScope IMemberOrNamedType.ExecutionScope => ExecutionScope.RunTime;

    IMemberOrNamedType IMemberOrNamedType.Definition => this;

    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => this.MemberOrNamedTypeBuilder.ToMemberOrNamedTypeRef();
}