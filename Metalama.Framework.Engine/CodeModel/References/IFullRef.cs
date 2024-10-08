// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// Represents an <see cref="IRef{T}"/> that is bound to a compilation and therefore several additional members.
/// </summary>
internal interface IFullRef : IRefImpl
{
    ISymbol GetClosestContainingSymbol();

    /// <summary>
    /// Gets the <see cref="DeclarationKind"/> of the reference declaration, if available.
    /// </summary>
    DeclarationKind DeclarationKind { get; }

    IFullRef? ContainingDeclaration { get; }

    /// <summary>
    /// Gets the name of the referenced declaration, if available. 
    /// </summary>
    string? Name { get; }

    new IFullRef<TOut> As<TOut>()
        where TOut : class, ICompilationElement;

    CompilationContext CompilationContext { get; }

    ResolvedAttributeRef GetAttributeData();

    bool IsDefinition { get; }

    void EnumerateAttributes( CompilationModel compilation, Action<IRef<IAttribute>> add );

    void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    IEnumerable<IFullRef> GetMembersOfName( string name, DeclarationKind kind, CompilationModel compilation );

    IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation );

    bool IsConvertibleTo( IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default );

    IAssemblySymbol GetAssemblySymbol( CompilationContext compilationContext );

    bool IsStatic { get; }

    /// <summary>
    /// Gets a the property or event from an accessor. 
    /// </summary>
    IFullRef<IMember> TypeMember { get; }

    MethodKind MethodKind { get; }

    /// <summary>
    /// Gets a value indicating whether the `return` statements of the method should have some argument.
    /// </summary>
    bool MethodBodyReturnsVoid { get; }

    IFullRef<INamedType>? DeclaringType { get; }

    IFullRef<ICompilation> Compilation { get; }

    bool IsPrimaryConstructor { get; }

    /// <summary>
    /// Determines whether the type is valid.
    /// </summary>
    bool IsValid { get; }
}

internal interface IFullRef<out T> : IFullRef, IRef<T> where T : class, ICompilationElement
{
    IFullRef<T> Definition { get; }

    IFullRef<T> WithGenericContext( GenericContext genericContext );
}