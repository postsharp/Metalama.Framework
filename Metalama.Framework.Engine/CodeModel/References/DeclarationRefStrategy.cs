// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class DeclarationRefStrategy : IRefStrategy
{
    public void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<IRef<IAttribute>> add )
    {
        var builderRef = (IDeclarationRef) declaration;

        foreach ( var attribute in builderRef.Declaration.Attributes )
        {
            add( attribute.ToRef() );
        }
    }

    public void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        var resolvedNameType = (INamedType) ((IBuilderRef) namedType).Builder;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i.ToRef() );
        }
    }

    public void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        // BUG: EnumerateAllImplementedInterfaces and EnumerateImplementedInterfaces should not have the same implementation.

        var resolvedNameType = (INamedType) ((IBuilderRef) namedType).Builder;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i.ToRef() );
        }
    }

    private static INamedDeclarationCollection<T> GetCollection<T>( IDeclaration parent, DeclarationKind kind )
        where T : class, INamedDeclaration
    {
        return kind switch
        {
            DeclarationKind.Event => (INamedDeclarationCollection<T>) ((INamedType) parent).Events,
            DeclarationKind.Constructor => (INamedDeclarationCollection<T>) ((INamedType) parent).Constructors,
            DeclarationKind.Field => (INamedDeclarationCollection<T>) ((INamedType) parent).Fields,
            DeclarationKind.Indexer => (INamedDeclarationCollection<T>) ((INamedType) parent).Indexers,
            DeclarationKind.Method => (INamedDeclarationCollection<T>) ((INamedType) parent).Methods,
            DeclarationKind.Property => (INamedDeclarationCollection<T>) ((INamedType) parent).Properties,
            DeclarationKind.Namespace => (INamedDeclarationCollection<T>) ((INamespace) parent).Namespaces,
            _ => throw new ArgumentOutOfRangeException( nameof(kind) )
        };
    }

    public IEnumerable<IRef<T>> GetMembersOfName<T>( IRef<INamespaceOrNamedType> parent, string name, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration
    {
        var parentDeclaration = ((IDeclarationRef) parent).Declaration;

        var collection = GetCollection<T>( parentDeclaration, kind );

        return collection.OfName( name ).Select( x => x.ToRef().As<T>() );
    }

    public IEnumerable<IRef<T>> GetMembers<T>( IRef<INamespaceOrNamedType> parent, DeclarationKind kind, CompilationModel compilation )
        where T : class, INamedDeclaration
    {
        var parentDeclaration = ((IDeclarationRef) parent).Declaration;

        var collection = GetCollection<T>( parentDeclaration, kind );

        return collection.SelectAsReadOnlyCollection( x => x.ToRef().As<T>() );
    }

    public bool Is( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
        => throw new NotImplementedException();

    public static IRefStrategy Instance { get; } = new DeclarationRefStrategy();
}