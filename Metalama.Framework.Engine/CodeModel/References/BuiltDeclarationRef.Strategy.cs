// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class BuiltDeclarationRef<T>
{
    public override void EnumerateAttributes( CompilationModel compilation, Action<AttributeRef> add )
    {
        foreach ( var attribute in this.BuilderData.Attributes )
        {
            add( attribute.ToRef() );
        }
    }

    public override void EnumerateAllImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
    {
        Invariant.Assert( this is IRef<INamedType> );

        var resolvedNameType = (NamedTypeBuilderData) this.BuilderData;

        foreach ( var i in resolvedNameType.ImplementedInterfaces )
        {
            add( i );
        }
    }

    public override void EnumerateImplementedInterfaces( CompilationModel compilation, Action<IFullRef<INamedType>> add )
    {
        Invariant.Assert( this is IRef<INamedType> );

        // BUG: EnumerateAllImplementedInterfaces and EnumerateImplementedInterfaces should not have the same implementation.

        var resolvedNameType = (NamedTypeBuilderData) this.BuilderData;

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

    public override IEnumerable<IFullRef> GetMembersOfName(
        string name,
        DeclarationKind kind,
        CompilationModel compilation )
    {
        var parentDeclaration = compilation.Factory.GetDeclaration( this.BuilderData );

        var collection = GetCollection( parentDeclaration, kind );

        return collection.OfName( name ).Select( x => x.ToFullRef() );
    }

    public override IEnumerable<IFullRef> GetMembers( DeclarationKind kind, CompilationModel compilation )
    {
        var parentDeclaration = compilation.Factory.GetDeclaration( this.BuilderData );

        var collection = GetCollection( parentDeclaration, kind );

        return collection.SelectAsReadOnlyCollection( x => x.ToFullRef() );
    }

    public override bool IsConvertibleTo( IRef<IType> right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
        => throw new NotImplementedException();

    public override IAssemblySymbol GetAssemblySymbol( CompilationContext compilationContext ) => compilationContext.Compilation.Assembly;

    public override bool IsStatic
    {
        get
        {
            Invariant.Assert( this is IRef<IMember> );

            return ((MemberBuilderData) this.BuilderData).IsStatic;
        }
    }

    public override IFullRef<IMember> TypeMember
    {
        get
        {
            Invariant.Assert( this is IRef<IMember> );

            return this.BuilderData switch
            {
                MethodBuilderData
                {
                    MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise
                    or MethodKind.PropertyGet or MethodKind.PropertySet
                } methodBuilderData => methodBuilderData.ContainingDeclaration.As<IMember>(),
                _ => this.As<IMember>()
            };
        }
    }

    public override MethodKind MethodKind
    {
        get
        {
            Invariant.Assert( this is IRef<IMethod> );

            return ((MethodBuilderData) this.BuilderData).MethodKind;
        }
    }

    public override bool MethodBodyReturnsVoid
    {
        get
        {
            Invariant.Assert( this is IRef<IMethod> );

            var builderData = (MethodBuilderData) this.BuilderData;
            var returnTypeSymbol = (ITypeSymbol) ((ISymbolRef) builderData.ReturnParameter.Type).Symbol;

            if ( builderData.IsAsync )
            {
                if ( AsyncHelper.TryGetAsyncInfo( returnTypeSymbol, out var returnStatementType, out _ ) )
                {
                    return returnStatementType.SpecialType == SpecialType.System_Void;
                }
            }

            return returnTypeSymbol.SpecialType == SpecialType.System_Void;
        }
    }

    public override IFullRef<INamedType> DeclaringType => this.BuilderData.DeclaringType.AssertNotNull();

    public override bool IsPrimaryConstructor => false;

    public override bool IsValid => true;
}