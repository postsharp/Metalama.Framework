// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface IRefStrategy
{
    void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<IRef<IAttribute>> add );

    void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add );

    void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add );

    IEnumerable<IRef<T>> GetMembersOfName<T>( IRef<INamespaceOrNamedType> parent, string name, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration;

    IEnumerable<IRef<T>> GetMembers<T>( IRef<INamespaceOrNamedType> parent, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration;

    bool Is( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default );
}