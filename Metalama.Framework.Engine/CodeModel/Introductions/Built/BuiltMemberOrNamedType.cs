// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal abstract class BuiltMemberOrNamedType : BuiltNamedDeclaration, IMemberOrNamedTypeImpl
{
    protected BuiltMemberOrNamedType( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder { get; }

    public Accessibility Accessibility => this.MemberOrNamedTypeBuilder.Accessibility;

    public bool IsAbstract => this.MemberOrNamedTypeBuilder.IsAbstract;

    public bool IsStatic => this.MemberOrNamedTypeBuilder.IsStatic;

    public bool IsSealed => this.MemberOrNamedTypeBuilder.IsSealed;

    public bool IsNew => this.MemberOrNamedTypeBuilder.IsNew;

    public bool? HasNewKeyword => this.MemberOrNamedTypeBuilder.HasNewKeyword;

    public bool IsPartial => this.MemberOrNamedTypeBuilder.IsPartial;

    [Memo]
    public INamedType? DeclaringType => this.MapDeclaration( this.MemberOrNamedTypeBuilder.DeclaringType );

    public MemberInfo ToMemberInfo() => throw new NotImplementedException();

    ExecutionScope IMemberOrNamedType.ExecutionScope => ExecutionScope.RunTime;

    IMemberOrNamedType IMemberOrNamedType.Definition => this.GetDefinition();

    protected abstract IMemberOrNamedType GetDefinition();

    IRef<IMemberOrNamedType> IMemberOrNamedType.ToRef() => (IRef<IMemberOrNamedType>) this.ToDeclarationRef();
}