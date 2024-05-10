// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltMemberOrNamedType : BuiltDeclaration, IMemberOrNamedType
{
    protected BuiltMemberOrNamedType( CompilationModel compilation, MemberOrNamedTypeBuilder builder ) : base( compilation, builder ) { }

    protected abstract MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder { get; }

    public Accessibility Accessibility => this.MemberOrNamedTypeBuilder.Accessibility;

    public string Name => this.MemberOrNamedTypeBuilder.Name;

    public bool IsAbstract => this.MemberOrNamedTypeBuilder.IsAbstract;

    public bool IsStatic => this.MemberOrNamedTypeBuilder.IsStatic;

    public bool IsSealed => this.MemberOrNamedTypeBuilder.IsSealed;

    public bool IsNew => this.MemberOrNamedTypeBuilder.IsNew;

    public bool? HasNewKeyword => this.MemberOrNamedTypeBuilder.HasNewKeyword;

    public INamedType? DeclaringType => this.Compilation.Factory.GetDeclaration( this.MemberOrNamedTypeBuilder.DeclaringType, ReferenceResolutionOptions.CanBeMissing );

    public MemberInfo ToMemberInfo() => this.MemberOrNamedTypeBuilder.ToMemberInfo();

    ExecutionScope IMemberOrNamedType.ExecutionScope => ExecutionScope.RunTime;

    IMemberOrNamedType IMemberOrNamedType.Definition => this;
}