// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// Methods used by <see cref="DeclarationUpdatableCollection{T}"/>, whose implementation
/// differs by kinds of references.
/// </summary>
internal interface IRefStrategy
{
    void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<AttributeRef> add );

    void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add );

    void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add );

    IEnumerable<IRef> GetMembersOfName( IRef parent, string name, DeclarationKind kind, CompilationModel compilation );

    IEnumerable<IRef> GetMembers( IRef parent, DeclarationKind kind, CompilationModel compilation );

    bool IsConvertibleTo( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default );
}