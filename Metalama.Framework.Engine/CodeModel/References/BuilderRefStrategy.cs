// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// Implementation of <see cref="IRefStrategy"/> for <see cref="BuiltDeclarationRef{T}"/>.
/// </summary>
internal sealed class BuilderRefStrategy : IRefStrategy
{
    public void EnumerateAttributes( IRef<IDeclaration> declaration, CompilationModel compilation, Action<AttributeRef> add )
    {
        var builderRef = (IBuiltDeclarationRef) declaration;

        foreach ( var attribute in builderRef.BuilderData.Attributes )
        {
            add( (AttributeRef) attribute.ToRef() );
        }
    }

    public void EnumerateAllImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        var resolvedNameType = (NamedTypeBuilderData) ((IBuiltDeclarationRef) namedType).BuilderData;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i );
        }
    }

    public void EnumerateImplementedInterfaces( IRef<INamedType> namedType, CompilationModel compilation, Action<IRef<INamedType>> add )
    {
        // BUG: EnumerateAllImplementedInterfaces and EnumerateImplementedInterfaces should not have the same implementation.

        var resolvedNameType = (NamedTypeBuilderData) ((IBuiltDeclarationRef) namedType).BuilderData;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i );
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
        var parentDeclaration = compilation.Factory.GetDeclaration( ((IBuiltDeclarationRef) parent).BuilderData );

        var collection = GetCollection( parentDeclaration, kind );

        return collection.OfName( name ).Select( x => x.ToRef() );
    }

    public IEnumerable<IRef> GetMembers( IRef parent, DeclarationKind kind, CompilationModel compilation )
    {
        var parentDeclaration = compilation.Factory.GetDeclaration( ((IBuiltDeclarationRef) parent).BuilderData );

        var collection = GetCollection( parentDeclaration, kind );

        return collection.SelectAsReadOnlyCollection( x => x.ToRef() );
    }

    public bool IsConvertibleTo( IRef<IType> left, IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
        => throw new NotImplementedException();

    public IAssemblySymbol GetAssemblySymbol( IRef reference, CompilationContext compilationContext ) => compilationContext.Compilation.Assembly;

    public bool IsStatic( IRef<IMember> reference ) => ((MemberBuilderData) ((IBuiltDeclarationRef) reference).BuilderData).IsStatic;

    public IRef<IMember> GetTypeMember( IRef<IMember> reference )
        => ((IBuiltDeclarationRef) reference).BuilderData switch
        {
            MethodBuilderData { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise
                    or MethodKind.PropertyGet or MethodKind.PropertySet
                } methodBuilderData => methodBuilderData.ContainingDeclaration.As<IMember>(),
            _ => reference
        };

    public static IRefStrategy Instance { get; } = new BuilderRefStrategy();
}