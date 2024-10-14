// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.CodeModel.Source.Types;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.Factories;

public partial class DeclarationFactory
{
    private readonly Cache<ISymbol, IDeclaration> _symbolCache;

// For types, we have a null-sensitive comparer to that 'object' and 'object?' are cached as two distinct items.
    private readonly Cache<ITypeSymbol, IType> _typeCache;

    private readonly record struct CreateFromSymbolArgs<TSymbol>( TSymbol Symbol, DeclarationFactory Factory, GenericContext GenericContext )
    {
        public CompilationModel Compilation => this.Factory._compilationModel;
    }

    private delegate TDeclaration CreateFromSymbolDelegate<out TDeclaration, TSymbol>( in CreateFromSymbolArgs<TSymbol> args );

    private TDeclaration GetDeclarationFromSymbol<TDeclaration, TSymbol>(
        TSymbol symbol,
        CreateFromSymbolDelegate<TDeclaration, TSymbol> createDeclaration,
        bool supportsRedirection = false )
        where TSymbol : ISymbol
        where TDeclaration : class, IDeclaration
    {
        using ( StackOverflowHelper.Detect() )
        {
            symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

            return (TDeclaration) this._symbolCache.GetOrAdd(
                symbol,
                GenericContext.Empty,
                typeof(TDeclaration),
                static ( _, _, x ) =>
                {
                    if ( x.supportsRedirection && x.me._compilationModel.TryGetRedirectedDeclaration(
                            x.me._compilationModel.RefFactory.FromSymbol<TDeclaration>( x.symbol.OriginalDefinition ),
                            out var redirectedDefinition ) )
                    {
                        var genericContext = GenericContext.Get( x.symbol, x.me.CompilationContext );

                        return x.me.GetDeclaration( redirectedDefinition, genericContext, typeof(TDeclaration) );
                    }

                    return x.createDeclaration( new CreateFromSymbolArgs<TSymbol>( x.symbol, x.me, GenericContext.Empty ) );
                },
                (me: this, symbol, createDeclaration, supportsRedirection) );
        }
    }

    private TType GetTypeFromSymbol<TType, TSymbol>(
        TSymbol symbol,
        CreateFromSymbolDelegate<TType, TSymbol> createType,
        bool supportsRedirection = false )
        where TSymbol : ITypeSymbol
        where TType : class, IType
    {
        using ( StackOverflowHelper.Detect() )
        {
            symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

            return (TType) this._typeCache.GetOrAdd(
                symbol,
                GenericContext.Empty,
                typeof(IType),
                static ( _, _, x ) => x.createDeclaration( new CreateFromSymbolArgs<TSymbol>( x.symbol, x.me, GenericContext.Empty ) ),
                (me: this, symbol, createDeclaration: createType, supportsRedirection) );
        }
    }

    private TType GetTypeFromSymbol<TType, TSymbol>(
        TSymbol symbol,
        GenericContext genericContext,
        CreateFromSymbolDelegate<TType, TSymbol> createType,
        bool supportsRedirection = false )
        where TSymbol : ITypeSymbol
        where TType : class, IType
    {
        using ( StackOverflowHelper.Detect() )
        {
            symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

            return (TType) this._typeCache.GetOrAdd(
                symbol,
                genericContext,
                typeof(IType),
                static ( _, _, x ) => x.createDeclaration( new CreateFromSymbolArgs<TSymbol>( x.symbol, x.me, x.genericContext ) ),
                (me: this, symbol, createDeclaration: createType, supportsRedirection, genericContext) );
        }
    }

    internal INamespace GetNamespace( INamespaceSymbol namespaceSymbol )
        => this.GetDeclarationFromSymbol<INamespace, INamespaceSymbol>(
            namespaceSymbol,
            static ( in CreateFromSymbolArgs<INamespaceSymbol> args ) => new SourceNamespace( args.Symbol, args.Compilation ) );

    internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
        => this.GetDeclarationFromSymbol<IAssembly, IAssemblySymbol>(
            assemblySymbol,
            static ( in CreateFromSymbolArgs<IAssemblySymbol> args ) =>
                !args.Symbol.Identity.Equals( args.Compilation.RoslynCompilation.Assembly.Identity )
                    ? new ExternalAssembly( args.Symbol, args.Compilation )
                    : args.Compilation );

    public IType GetIType( ITypeSymbol typeSymbol )
        => typeSymbol switch
        {
            // TODO PERF: switch by SymbolKind.
            INamedTypeSymbol namedType => this.GetNamedType( namedType ),
            IArrayTypeSymbol arrayType => this.GetArrayType( arrayType ),
            IPointerTypeSymbol pointerType => this.GetPointerType( pointerType ),
            ITypeParameterSymbol typeParameter => this.GetTypeParameter( typeParameter ),
            IDynamicTypeSymbol dynamicType => this.GetDynamicType( dynamicType ),
            IFunctionPointerTypeSymbol functionPointerType => this.GetFunctionPointerType( functionPointerType ),
            _ => throw new NotImplementedException( $"Types of kind {typeSymbol.Kind} are not implemented." )
        };

    private IArrayType GetArrayType( IArrayTypeSymbol typeSymbol )
        => this.GetTypeFromSymbol<IArrayType, IArrayTypeSymbol>(
            typeSymbol,
            static ( in CreateFromSymbolArgs<IArrayTypeSymbol> args ) => new SymbolArrayType( args.Symbol, args.Compilation ) );

    private IDynamicType GetDynamicType( IDynamicTypeSymbol typeSymbol )
        => this.GetTypeFromSymbol<IDynamicType, IDynamicTypeSymbol>(
            typeSymbol,
            static ( in CreateFromSymbolArgs<IDynamicTypeSymbol> args ) => new DynamicType( args.Symbol, args.Compilation ) );

    private IPointerType GetPointerType( IPointerTypeSymbol typeSymbol )
        => this.GetTypeFromSymbol<IPointerType, IPointerTypeSymbol>(
            typeSymbol,
            static ( in CreateFromSymbolArgs<IPointerTypeSymbol> args ) => new SymbolPointerType( args.Symbol, args.Compilation ) );

    private IFunctionPointerType GetFunctionPointerType( IFunctionPointerTypeSymbol typeSymbol )
        => this.GetTypeFromSymbol<IFunctionPointerType, IFunctionPointerTypeSymbol>(
            typeSymbol,
            static ( in CreateFromSymbolArgs<IFunctionPointerTypeSymbol> args ) => new SymbolFunctionPointerType( args.Symbol, args.Compilation ) );

    public INamedType GetNamedType( INamedTypeSymbol typeSymbol, IGenericContext? genericContext = null )
    {
        // Roslyn considers the type in e.g. typeof(List<>) to be different from e.g. List<T>.
        // That distinction makes things more complicated for us (e.g. it's not representable using Type), so get rid of it.
        if ( typeSymbol.IsUnboundGenericType )
        {
            typeSymbol = typeSymbol.ConstructedFrom;
        }

        if ( typeSymbol.Kind == SymbolKind.ErrorType )
        {
            return this.GetSpecialType( SpecialType.Object );
        }

        return this.GetTypeFromSymbol<INamedType, INamedTypeSymbol>(
            typeSymbol,
            static ( in CreateFromSymbolArgs<INamedTypeSymbol> args ) =>
                new SourceNamedType( args.Symbol, args.Compilation ) );
    }

    // We must use GetTypeFromSymbol and not GetDeclarationFromSymbol because of nullability.
    public ITypeParameter GetTypeParameter( ITypeParameterSymbol typeParameterSymbol )
        => this.GetTypeFromSymbol<ITypeParameter, ITypeParameterSymbol>(
            typeParameterSymbol,
            static ( in CreateFromSymbolArgs<ITypeParameterSymbol> args ) =>
                new SourceTypeParameter( args.Symbol, args.Compilation, args.GenericContext ) );

    private ITypeParameter GetTypeParameter( ITypeParameterSymbol typeParameterSymbol, GenericContext genericContext )
        => this.GetTypeFromSymbol<ITypeParameter, ITypeParameterSymbol>(
            typeParameterSymbol,
            genericContext,
            static ( in CreateFromSymbolArgs<ITypeParameterSymbol> args ) =>
                new SourceTypeParameter( args.Symbol, args.Compilation, args.GenericContext ) );

    public IMethod GetMethod( IMethodSymbol methodSymbol )
    {
        // Standardize on the partial definition part for partial methods.
        methodSymbol = methodSymbol.PartialDefinitionPart ?? methodSymbol;

        return this.GetDeclarationFromSymbol<IMethod, IMethodSymbol>(
            methodSymbol,
            static ( in CreateFromSymbolArgs<IMethodSymbol> args ) =>
                new SourceMethod( args.Symbol, args.Compilation ) );
    }

    public IProperty GetProperty( IPropertySymbol propertySymbol )
        => this.GetDeclarationFromSymbol<IProperty, IPropertySymbol>(
            propertySymbol,
            static ( in CreateFromSymbolArgs<IPropertySymbol> args ) =>
                new SourceProperty( args.Symbol, args.Compilation ) );

    public IIndexer GetIndexer( IPropertySymbol propertySymbol )
        => this.GetDeclarationFromSymbol<IIndexer, IPropertySymbol>(
            propertySymbol,
            static ( in CreateFromSymbolArgs<IPropertySymbol> args ) =>
                new SourceIndexer( args.Symbol, args.Compilation ) );

    // Fields support redirections, but fields redirect to properties, so it is not handled at this level.
    public IField GetField( IFieldSymbol fieldSymbol )
        => this.GetDeclarationFromSymbol<IField, IFieldSymbol>(
            fieldSymbol,
            static ( in CreateFromSymbolArgs<IFieldSymbol> args ) =>
                new SourceField( args.Symbol, args.Compilation ) );

    internal IField GetField( IFieldSymbol fieldSymbol, GenericContext genericContext )
    {
        return this.GetField( (IFieldSymbol) genericContext.Map( fieldSymbol ) );
    }

    public IConstructor GetConstructor( IMethodSymbol methodSymbol )
        => this.GetDeclarationFromSymbol<IConstructor, IMethodSymbol>(
            methodSymbol,
            static ( in CreateFromSymbolArgs<IMethodSymbol> args ) =>
                new SourceConstructor( args.Symbol, args.Compilation ),
            true );

    public IParameter GetParameter( IParameterSymbol parameterSymbol )
        => this.GetDeclarationFromSymbol<IParameter, IParameterSymbol>(
            parameterSymbol,
            static ( in CreateFromSymbolArgs<IParameterSymbol> args ) =>
                new SourceParameter( args.Symbol, args.Compilation ) );

    public IEvent GetEvent( IEventSymbol eventSymbol )
        => this.GetDeclarationFromSymbol<IEvent, IEventSymbol>(
            eventSymbol,
            static ( in CreateFromSymbolArgs<IEventSymbol> args ) =>
                new SourceEvent( args.Symbol, args.Compilation ) );

    public bool TryGetDeclaration( ISymbol symbol, [NotNullWhen( true )] out IDeclaration? declaration )
    {
        var compilationElement = this.GetCompilationElement( symbol );
        declaration = compilationElement as IDeclaration;

        return declaration != null;
    }

    internal IDeclaration? GetDeclarationOrNull( ISymbol symbol, RefTargetKind kind = RefTargetKind.Default )
        => this.GetCompilationElement( symbol, kind ) as IDeclaration;

    public IDeclaration GetDeclaration( SymbolDictionaryKey key ) => this.GetDeclaration( key.GetSymbolId().Resolve( this.Compilation ).AssertSymbolNotNull() );

    public IDeclaration GetDeclaration( ISymbol symbol ) => this.GetDeclaration( symbol, RefTargetKind.Default );

    internal IDeclaration GetDeclaration( ISymbol symbol, RefTargetKind kind, GenericContext? genericContext = null )
    {
        var compilationElement = this.GetCompilationElement( symbol, kind, genericContext );

        if ( compilationElement is not IDeclaration declaration )
        {
            throw new ArgumentException( $"{symbol.Kind} is not a declaration", nameof(symbol) );
        }

        return declaration;
    }

    internal ICompilationElement? GetCompilationElement(
        ISymbol symbol,
        RefTargetKind targetKind = RefTargetKind.Default,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Local:
            case SymbolKind.Label:
            case SymbolKind.ErrorType:
                return null;
        }

        symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

        var typedGenericContext = genericContext.AsGenericContext();

        var mappedSymbol = genericContext.AsGenericContext().Map( symbol );

        if ( interfaceType == typeof(ITypeParameter) )
        {
            // We intentionally don't pass the mapped symbol because it may be mapped to a non-ITypeParameterSymbol.
            // The Roslyn code model does not support "generic instances" of type parameters, the support for this feature in
            // our code model is done in the TypeParameter class.

            return this.GetTypeParameter( (ITypeParameterSymbol) symbol, typedGenericContext );
        }
        else
        {
            return this.GetCompilationElementCore( mappedSymbol, targetKind, typedGenericContext, interfaceType );
        }
    }

    private ICompilationElement? GetCompilationElementCore(
        ISymbol mappedSymbol,
        RefTargetKind targetKind,
        GenericContext genericContext,
        Type? interfaceType )
    {
        switch ( mappedSymbol.Kind )
        {
            case SymbolKind.NamedType:
                {
                    var type = this.GetNamedType( (INamedTypeSymbol) mappedSymbol );

                    return targetKind switch
                    {
                        RefTargetKind.StaticConstructor => type.StaticConstructor,
                        RefTargetKind.Default => type,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.ArrayType:
                return this.GetArrayType( (IArrayTypeSymbol) mappedSymbol );

            case SymbolKind.PointerType:
                return this.GetPointerType( (IPointerTypeSymbol) mappedSymbol );

            case SymbolKind.DynamicType:
                return this.GetDynamicType( (IDynamicTypeSymbol) mappedSymbol );

            case SymbolKind.Method:
                {
                    var method = (IMethodSymbol) mappedSymbol;

                    return
                        targetKind switch
                        {
                            RefTargetKind.Return => this.GetReturnParameter( method ),
                            RefTargetKind.Parameter => this.GetParameter( method.Parameters[0] ),
                            RefTargetKind.Default => method.GetDeclarationKind( this.CompilationContext ) switch
                            {
                                DeclarationKind.Method or DeclarationKind.Finalizer => this.GetMethod( method ),
                                DeclarationKind.Constructor => this.GetConstructor( method ),
                                _ => throw new AssertionFailedException(
                                    $"Unexpected DeclarationRefTargetKind: {method.GetDeclarationKind( this.CompilationContext )}." )
                            },
                            _ => throw new AssertionFailedException( $"Unexpected TargetKind for method: {targetKind}" )
                        };
                }

            case SymbolKind.Property:
                var propertySymbol = (IPropertySymbol) mappedSymbol;

                var propertyOrIndexer = propertySymbol.IsIndexer
                    ? (IPropertyOrIndexer) this.GetIndexer( propertySymbol )
                    : this.GetProperty( propertySymbol );

                return targetKind switch
                {
                    // Implicit getter or setter.
                    RefTargetKind.PropertyGet => propertyOrIndexer.GetMethod,
                    RefTargetKind.PropertySet => propertyOrIndexer.SetMethod,

                    // The property itself.
                    RefTargetKind.Default => propertyOrIndexer,
                    RefTargetKind.Property => propertyOrIndexer,

                    // The underlying field.
                    RefTargetKind.Field => this.GetField( propertySymbol.GetBackingField().AssertSymbolNotNull() ),

                    _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                };

            case SymbolKind.Field:
                {
                    var field = this.GetField( (IFieldSymbol) mappedSymbol );

                    return targetKind switch
                    {
                        RefTargetKind.Default => field,
                        RefTargetKind.Field => field,
                        RefTargetKind.PropertyGet => field.GetMethod,
                        RefTargetKind.PropertySet => field.SetMethod,
                        RefTargetKind.PropertyGetReturnParameter => field.GetMethod?.ReturnParameter,
                        RefTargetKind.PropertySetParameter => field.SetMethod?.Parameters[0],
                        RefTargetKind.PropertySetReturnParameter => field.SetMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.TypeParameter:
                return this.GetTypeParameter( (ITypeParameterSymbol) mappedSymbol );

            case SymbolKind.Parameter:
                return this.GetParameter( (IParameterSymbol) mappedSymbol );

            case SymbolKind.Event:
                {
                    var @event = this.GetEvent( (IEventSymbol) mappedSymbol );

                    return targetKind switch
                    {
                        RefTargetKind.Default => @event,
                        RefTargetKind.EventRaise => @event.RaiseMethod,
                        RefTargetKind.EventRaiseParameter => throw new NotImplementedException(),
                        RefTargetKind.EventRaiseReturnParameter => @event.RaiseMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.Assembly:
                return this.GetAssembly( (IAssemblySymbol) mappedSymbol );

            case SymbolKind.Namespace:
                return this.GetNamespace( (INamespaceSymbol) mappedSymbol );

            case SymbolKind.NetModule:
                return this._compilationModel;

            default:
                throw new AssertionFailedException( $"Don't know how to resolve a '{mappedSymbol.Kind}'." );
        }
    }

    public IParameter GetReturnParameter( IMethodSymbol methodSymbol ) => this.GetMethod( methodSymbol ).ReturnParameter;

    public IAssembly GetAssembly( AssemblyIdentity assemblyIdentity )
    {
        if ( this.Compilation.Assembly.Identity.Equals( assemblyIdentity ) )
        {
            return this._compilationModel;
        }
        else
        {
            // TODO: performance
            var assemblySymbol = this.Compilation.SourceModule.ReferencedAssemblySymbols.Single( a => a.Identity.Equals( assemblyIdentity ) );

            return this.GetAssembly( assemblySymbol );
        }
    }
}