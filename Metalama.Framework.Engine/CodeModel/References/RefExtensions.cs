// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using DeclarationKind = Metalama.Framework.Code.DeclarationKind;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    internal static IRefStrategy GetStrategy( this IRef reference ) => ((IRefImpl) reference).Strategy;

    // TODO: Portable references are useful only in design-time execution scenarios.
    internal static IRef<T> ToPortable<T>( this IRef<T> reference )
        where T : class, ICompilationElement
        => ((IRefImpl<T>) reference).ToPortable();

    internal static IRef<T> AssertPortable<T>( this IRef<T> reference )
        where T : class, ICompilationElement
    {
#if DEBUG
        if ( !((IRefImpl) reference).IsPortable )
        {
            throw new AssertionFailedException( $"The reference '{reference}' must be portable." );
        }
#endif

        return reference;
    }

    internal static IRef ToPortable( this IRef reference ) => ((IRefImpl) reference).ToPortable();

    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree<T>( this T reference, CompilationContext compilationContext )
        where T : IRef<IDeclaration>
        => ((IRefImpl) reference).GetClosestContainingSymbol( compilationContext ).GetPrimarySyntaxReference()?.SyntaxTree;

    internal static Type GetRefInterfaceType( this DeclarationKind declarationKind, RefTargetKind refTargetKind = RefTargetKind.Default )
        => refTargetKind switch
        {
            RefTargetKind.Return => typeof(IParameter),
            RefTargetKind.Assembly => typeof(IAssembly),
            RefTargetKind.Module => typeof(IAssembly),
            RefTargetKind.Field => typeof(IFieldOrProperty),
            RefTargetKind.Parameter => typeof(IParameter),
            RefTargetKind.Property => typeof(IProperty),
            RefTargetKind.Event => typeof(IEvent),
            RefTargetKind.PropertyGet => typeof(IMethod),
            RefTargetKind.PropertySet => typeof(IMethod),
            RefTargetKind.StaticConstructor => typeof(IConstructor),
            RefTargetKind.PropertySetParameter => typeof(IParameter),
            RefTargetKind.PropertyGetReturnParameter => typeof(IParameter),
            RefTargetKind.PropertySetReturnParameter => typeof(IParameter),
            RefTargetKind.EventRaise => typeof(IMethod),
            RefTargetKind.EventRaiseParameter => typeof(IParameter),
            RefTargetKind.EventRaiseReturnParameter => typeof(IParameter),
            RefTargetKind.NamedType => typeof(INamedType),
            RefTargetKind.Default => declarationKind switch
            {
                DeclarationKind.Type => typeof(IType),
                DeclarationKind.Compilation => typeof(ICompilation),
                DeclarationKind.NamedType => typeof(INamedType),
                DeclarationKind.Method => typeof(IMethod),
                DeclarationKind.Property => typeof(IProperty),
                DeclarationKind.Indexer => typeof(IIndexer),
                DeclarationKind.Field => typeof(IFieldOrProperty),
                DeclarationKind.Event => typeof(IEvent),
                DeclarationKind.Parameter => typeof(IParameter),
                DeclarationKind.TypeParameter => typeof(ITypeParameter),
                DeclarationKind.Attribute => typeof(IAttribute),
                DeclarationKind.ManagedResource => typeof(IManagedResource),
                DeclarationKind.Constructor => typeof(IConstructor),
                DeclarationKind.Finalizer => typeof(IMethod),
                DeclarationKind.Operator => typeof(IMethod),
                DeclarationKind.AssemblyReference => typeof(IAssembly),
                DeclarationKind.Namespace => typeof(INamespace),
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

    /*
    internal static IRef<TDeclaration> ToRef<TDeclaration>( this ISymbol symbol, CompilationContext compilationContext )
        where TDeclaration : class, IDeclaration
        => compilationContext.RefFactory.FromSymbol<TDeclaration>( symbol );
        */

    internal static IRef<IType> ToRef( this ITypeSymbol symbol, CompilationContext compilationContext )
        => symbol.Kind switch
        {
            SymbolKind.TypeParameter => compilationContext.RefFactory.FromSymbol<ITypeParameter>( symbol ),
            SymbolKind.NamedType => compilationContext.RefFactory.FromSymbol<INamedType>( symbol ),
            _ => compilationContext.RefFactory.FromSymbol<IType>( symbol )
        };
}