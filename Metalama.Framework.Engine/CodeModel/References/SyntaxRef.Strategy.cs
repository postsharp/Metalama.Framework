// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class SyntaxRef<T>
{
    public override string? Name => throw new NotImplementedException();

    public override void EnumerateAttributes( CompilationModel compilation, Action<AttributeRef> add ) => throw new NotImplementedException();

    public override void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
        => throw new NotImplementedException();

    public override void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
        => throw new NotImplementedException();

    public override IEnumerable<IFullRef> GetMembersOfName( string name, DeclarationKind kind, CompilationModel compilation )
        => throw new NotImplementedException();

    public override IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation ) => throw new NotImplementedException();

    public override bool IsConvertibleTo( IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
        => throw new NotImplementedException();

    public override IAssemblySymbol GetAssemblySymbol( CompilationContext compilationContext ) => throw new NotImplementedException();

    public override bool IsStatic => throw new NotImplementedException();

    public override IFullRef<IMember> TypeMember => throw new NotImplementedException();

    public override MethodKind MethodKind => throw new NotImplementedException();

    public override bool MethodBodyReturnsVoid => throw new NotImplementedException();

    public override IFullRef<INamedType> DeclaringType => throw new NotImplementedException();

    public override bool IsPrimaryConstructor => throw new NotImplementedException();
}