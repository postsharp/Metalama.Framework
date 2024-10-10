// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

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

    IFullRef<INamedType>? DeclaringType { get; }

    /// <summary>
    /// Gets the name of the referenced declaration, if available. 
    /// </summary>
    string? Name { get; }

    new IFullRef<TOut> As<TOut>()
        where TOut : class, ICompilationElement;

    CompilationContext CompilationContext { get; }

    RefFactory RefFactory { get; }

    ResolvedAttributeRef GetAttributeData();

    bool IsDefinition { get; }

    void EnumerateAttributes( CompilationModel compilation, Action<AttributeRef> add );

    void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add );

    IEnumerable<IFullRef> GetMembersOfName( string name, DeclarationKind kind, CompilationModel compilation );

    IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation );
}

internal interface IFullRef<out T> : IFullRef, IRef<T>
    where T : class, ICompilationElement
{
    IFullRef<T> DefinitionRef { get; }

    IFullRef<T> WithGenericContext( GenericContext genericContext );

    /// <summary>
    /// Gets the <see cref="IDeclaration"/> in the canonical <see cref="CompilationModel"/> of the current <see cref="RefFactory"/>.
    /// Try to use this property instead of <see cref="Declaration"/> when the generic context does not matter.
    /// </summary>
    T Definition { get; }
    
    T Declaration { get; }
}