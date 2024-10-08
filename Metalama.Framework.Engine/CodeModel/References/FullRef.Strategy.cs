// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References;

internal abstract partial class FullRef<T>
{
    public abstract DeclarationKind DeclarationKind { get; }

    public abstract IFullRef? ContainingDeclaration { get; }

    public abstract string? Name { get; }

    public abstract void EnumerateAttributes( CompilationModel compilation, Action<AttributeRef> add );

    public abstract void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    public abstract void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    public abstract IEnumerable<IFullRef> GetMembersOfName( string name, DeclarationKind kind, CompilationModel compilation );

    public abstract IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation );

    public abstract bool IsConvertibleTo( IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default );

    public abstract IAssemblySymbol GetAssemblySymbol( CompilationContext compilationContext );

    public abstract bool IsStatic { get; }

    public abstract IFullRef<IMember> TypeMember { get; }

    public abstract MethodKind MethodKind { get; }

    public abstract bool MethodBodyReturnsVoid { get; }

    public abstract IFullRef<INamedType> DeclaringType { get; }

    public IFullRef<ICompilation> Compilation => this.CompilationContext.RefFactory.ForCompilation();

    public abstract bool IsPrimaryConstructor { get; }

    public abstract bool IsValid { get; }
}