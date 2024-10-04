﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// Implementation of <see cref="IRefStrategy"/> for <see cref="BuilderRef{T}"/>.
/// </summary>
internal sealed class BuilderRefStrategy : IRefStrategy
{
    public void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<AttributeRef> add )
    {
        var builderRef = (IBuilderRef) declaration;

        foreach ( var attribute in builderRef.BuilderData.Attributes )
        {
            add( (AttributeRef) attribute.ToRef() );
        }
    }

    public void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        var resolvedNameType = (INamedType) ((IBuilderRef) namedType).BuilderData;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i.ToRef() );
        }
    }

    public void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        // BUG: EnumerateAllImplementedInterfaces and EnumerateImplementedInterfaces should not have the same implementation.

        var resolvedNameType = (INamedType) ((IBuilderRef) namedType).BuilderData;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i.ToRef() );
        }
    }

    private static INamedDeclarationCollection<INamedDeclaration> GetCollection( IDeclaration parent, DeclarationKind kind )
    {
        return kind switch
        {
            DeclarationKind.Event => ((INamedType) parent).Events,
            DeclarationKind.Constructor => ((INamedType) parent).Constructors,
            DeclarationKind.Field => ((INamedType) parent).Fields,
            DeclarationKind.Indexer => ((INamedType) parent).Indexers,
            DeclarationKind.Method => ((INamedType) parent).Methods,
            DeclarationKind.Property => ((INamedType) parent).Properties,
            DeclarationKind.Namespace => ((INamespace) parent).Namespaces,
            DeclarationKind.NamedType => ((INamespaceOrNamedType) parent).Types,
            _ => throw new ArgumentOutOfRangeException( nameof(kind) )
        };
    }

    public IEnumerable<IRef> GetMembersOfName(
        IRef parent,
        string name,
        DeclarationKind kind,
        CompilationModel compilation )
    {
        var parentDeclaration = ((IBuilderRef) parent).BuilderData;

        var collection = GetCollection( parentDeclaration, kind );

        return collection.OfName( name ).Select( x => x.ToRef() );
    }

    public IEnumerable<IRef> GetMembers( IRef parent, DeclarationKind kind, CompilationModel compilation )
    {
        var parentDeclaration = ((IBuilderRef) parent).BuilderData;

        var collection = GetCollection( parentDeclaration, kind );

        return collection.SelectAsReadOnlyCollection( x => x.ToRef() );
    }

    public bool IsConvertibleTo( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
        => throw new NotImplementedException();

    public static IRefStrategy Instance { get; } = new BuilderRefStrategy();
}