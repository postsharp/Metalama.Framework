// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using DeclarationKind = Metalama.Framework.Code.DeclarationKind;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    internal static IRefStrategy GetCollectionStrategy( this IRef reference ) => ((ICompilationBoundRefImpl) reference).Strategy;

    internal static IDurableRef<T> ToDurable<T>( this IRef<T> reference )
        where T : class, ICompilationElement
        => ((IRefImpl<T>) reference).ToDurable();

    internal static IRef ToDurable( this IRef reference ) => ((IRefImpl) reference).ToDurable();

    internal static bool IsConvertibleTo( this IRef<IType> type, IRef<IType> otherType, ConversionKind conversionKind = ConversionKind.Default )
        => type.GetCollectionStrategy().IsConvertibleTo( type, otherType, conversionKind );

    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree<T>( this T reference, CompilationContext compilationContext )
        where T : IRef<IDeclaration>
        => ((IRefImpl) reference).GetClosestContainingSymbol( compilationContext ).GetPrimarySyntaxReference()?.SyntaxTree;

    internal static Type[] GetPossibleDeclarationInterfaceTypes( this ISymbol symbol, CompilationContext compilationContext, RefTargetKind refTargetKind )
        => symbol.GetDeclarationKind( compilationContext ).GetPossibleDeclarationInterfaceTypes( refTargetKind );

    internal static Type[] GetPossibleDeclarationInterfaceTypes( this DeclarationKind declarationKind, RefTargetKind refTargetKind = RefTargetKind.Default )
        => refTargetKind switch
        {
            RefTargetKind.Return => [typeof(IParameter)],
            RefTargetKind.Assembly => [typeof(IAssembly)],
            RefTargetKind.Module => [typeof(IAssembly)],
            RefTargetKind.Field => [typeof(IField), typeof(IProperty)],
            RefTargetKind.Parameter => [typeof(IParameter)],
            RefTargetKind.Property => [typeof(IProperty)],
            RefTargetKind.Event => [typeof(IEvent)],
            RefTargetKind.PropertyGet => [typeof(IMethod)],
            RefTargetKind.PropertySet => [typeof(IMethod)],
            RefTargetKind.StaticConstructor => [typeof(IConstructor)],
            RefTargetKind.PropertySetParameter => [typeof(IParameter)],
            RefTargetKind.PropertyGetReturnParameter => [typeof(IParameter)],
            RefTargetKind.PropertySetReturnParameter => [typeof(IParameter)],
            RefTargetKind.EventRaise => [typeof(IMethod)],
            RefTargetKind.EventRaiseParameter => [typeof(IParameter)],
            RefTargetKind.EventRaiseReturnParameter => [typeof(IParameter)],
            RefTargetKind.NamedType => [typeof(INamedType)],
            RefTargetKind.Default => declarationKind switch
            {
                DeclarationKind.Type => [typeof(IType)],
                DeclarationKind.Compilation => [typeof(ICompilation)],
                DeclarationKind.NamedType => [typeof(INamedType)],
                DeclarationKind.Method => [typeof(IMethod)],
                DeclarationKind.Property => [typeof(IProperty), typeof(IField)],
                DeclarationKind.Indexer => [typeof(IIndexer)],
                DeclarationKind.Field => [typeof(IField), typeof(IProperty)],
                DeclarationKind.Event => [typeof(IEvent)],
                DeclarationKind.Parameter => [typeof(IParameter)],
                DeclarationKind.TypeParameter => [typeof(ITypeParameter)],
                DeclarationKind.Attribute => [typeof(IAttribute)],
                DeclarationKind.ManagedResource => [typeof(IManagedResource)],
                DeclarationKind.Constructor => [typeof(IConstructor)],
                DeclarationKind.Finalizer => [typeof(IMethod)],
                DeclarationKind.Operator => [typeof(IMethod)],
                DeclarationKind.AssemblyReference => [typeof(IAssembly)],
                DeclarationKind.Namespace => [typeof(INamespace)],
                _ => throw new ArgumentOutOfRangeException( nameof(declarationKind), declarationKind, null )
            },

            _ => throw new ArgumentOutOfRangeException( nameof(refTargetKind), refTargetKind, null )
        };

    internal static IRef<IDeclaration> ToRef( this ISymbol symbol, CompilationContext compilationContext )
        => compilationContext.RefFactory.FromDeclarationSymbol( symbol );

    internal static IRef<INamedType> ToRef( this INamedTypeSymbol symbol, CompilationContext compilationContext )
        => compilationContext.RefFactory.FromSymbol<INamedType>( symbol );

    internal static IRef<INamespace> ToRef( this INamespaceSymbol symbol, CompilationContext compilationContext )
        => compilationContext.RefFactory.FromSymbol<INamespace>( symbol );

    internal static IRef<IType> ToRef( this ITypeSymbol symbol, CompilationContext compilationContext )
        => symbol.Kind switch
        {
            SymbolKind.TypeParameter => compilationContext.RefFactory.FromSymbol<ITypeParameter>( symbol ),
            SymbolKind.NamedType => compilationContext.RefFactory.FromSymbol<INamedType>( symbol ),
            _ => compilationContext.RefFactory.FromSymbol<IType>( symbol )
        };

    internal static IEqualityComparer<ISymbol> GetSymbolComparer(
        this RefComparison comparison,
        CompilationContext compilationContext1,
        CompilationContext compilationContext2 )
        => comparison.GetSymbolComparer( compilationContext1 == compilationContext2 );

    internal static IEqualityComparer<ISymbol> GetSymbolComparer( this RefComparison comparison, bool useReferential = false )
        => comparison switch
        {
            RefComparison.Default => SymbolEqualityComparer.Default,
            RefComparison.Structural when useReferential => SymbolEqualityComparer.Default,
            RefComparison.Structural => StructuralSymbolComparer.Default,
            RefComparison.StructuralIncludeNullability when useReferential => SymbolEqualityComparer.IncludeNullability,
            RefComparison.StructuralIncludeNullability => StructuralSymbolComparer.IncludeNullability,
            RefComparison.IncludeNullability => SymbolEqualityComparer.IncludeNullability,
            _ => throw new ArgumentOutOfRangeException()
        };

    internal static bool IsDefinition( this IRef reference ) => ((ICompilationBoundRefImpl) reference).IsDefinition;

    internal static IRef<T> GetDefinition<T>( this IRef<T> reference )
        where T : class, IMemberOrNamedType
        => (IRef<T>) ((ICompilationBoundRefImpl) reference).Definition;

    internal static IRef GetDefinition( this IRef reference ) => ((ICompilationBoundRefImpl) reference).Definition;
}