// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DeclarationFactory
{
    private readonly ConcurrentDictionary<ISymbol, IDeclaration> _symbolCache;

    internal INamespace GetNamespace( INamespaceSymbol namespaceSymbol )
        => (INamespace) this._symbolCache.GetOrAdd(
            namespaceSymbol,
            static ( l, c ) => new Namespace( (INamespaceSymbol) l, c ),
            this._compilationModel );

    internal IAssembly GetAssembly( IAssemblySymbol assemblySymbol )
        => (IAssembly) this._symbolCache.GetOrAdd(
            assemblySymbol,
            static ( _, x )
                => !x.assemblySymbol.Identity.Equals( x.me._compilationModel.RoslynCompilation.Assembly.Identity )
                    ? new ExternalAssembly( x.assemblySymbol, x.me._compilationModel )
                    : x.me._compilationModel,
            (me: this, assemblySymbol) );

    public IType GetIType( ITypeSymbol typeSymbol )
        => this._typeCache.GetOrAdd(
            typeSymbol,
            static ( l, c ) => CodeModelFactory.CreateIType( l, c ),
            this._compilationModel );

    private IArrayType GetArrayType( IArrayTypeSymbol typeSymbol )
        => (ArrayType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new ArrayType( (IArrayTypeSymbol) s, c ),
            this._compilationModel );

    private IDynamicType GetDynamicType( IDynamicTypeSymbol typeSymbol )
        => (DynamicType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new DynamicType( (IDynamicTypeSymbol) s, c ),
            this._compilationModel );

    private IPointerType GetPointerType( IPointerTypeSymbol typeSymbol )
        => (PointerType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new PointerType( (IPointerTypeSymbol) s, c ),
            this._compilationModel );

    public INamedType GetNamedType( INamedTypeSymbol typeSymbol, bool translateToCurrentCompilation = false, IGenericContext? genericContext = null )
    {
        // Roslyn considers the type in e.g. typeof(List<>) to be different from e.g. List<T>.
        // That distinction makes things more complicated for us (e.g. it's not representable using Type), so get rid of it.
        if ( typeSymbol.IsUnboundGenericType )
        {
            typeSymbol = typeSymbol.ConstructedFrom;
        }

        if ( translateToCurrentCompilation )
        {
            typeSymbol = typeSymbol.TranslateIfNecessary( this.CompilationContext );
        }
        else
        {
            typeSymbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );
        }

        return (INamedType) this._typeCache.GetOrAdd(
            typeSymbol,
            static ( s, c ) => new NamedType( (INamedTypeSymbol) s, c ),
            this._compilationModel );
    }

    public ITypeParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
        => (TypeParameter) this._typeCache.GetOrAdd(
            typeParameterSymbol,
            static ( tp, c ) => new TypeParameter( (ITypeParameterSymbol) tp, c ),
            this._compilationModel );

    public IMethod GetMethod( IMethodSymbol methodSymbol )
    {
        // Standardize on the partial definition part for partial methods.
        methodSymbol = methodSymbol.PartialDefinitionPart ?? methodSymbol;

        return (IMethod) this._symbolCache.GetOrAdd(
            methodSymbol,
            static ( ms, c ) => new Method( (IMethodSymbol) ms, c ),
            this._compilationModel );
    }

    public IProperty GetProperty( IPropertySymbol propertySymbol )
        => (IProperty) this._symbolCache.GetOrAdd(
            propertySymbol,
            static ( ms, c ) => new Property( (IPropertySymbol) ms, c ),
            this._compilationModel );

    public IIndexer GetIndexer( IPropertySymbol propertySymbol )
        => (IIndexer) this._symbolCache.GetOrAdd(
            propertySymbol,
            static ( ms, c ) => new Indexer( (IPropertySymbol) ms, c ),
            this._compilationModel );

    public IField GetField( IFieldSymbol fieldSymbol )
        => (IField) this._symbolCache.GetOrAdd(
            fieldSymbol,
            static ( ms, c ) => new Field( (IFieldSymbol) ms, c ),
            this._compilationModel );

    public IConstructor GetConstructor( IMethodSymbol methodSymbol, bool translateToCurrentCompilation = false )
    {
        if ( translateToCurrentCompilation )
        {
            methodSymbol = methodSymbol.TranslateIfNecessary( this.CompilationContext );
        }

        return (IConstructor) this._symbolCache.GetOrAdd(
            methodSymbol,
            static ( ms, c ) => new Constructor( (IMethodSymbol) ms, c ),
            this._compilationModel );
    }

    public IMethod GetFinalizer( IMethodSymbol finalizerSymbol )
        => (IMethod) this._symbolCache.GetOrAdd(
            finalizerSymbol,
            static ( ms, c ) => new Method( (IMethodSymbol) ms, c ),
            this._compilationModel );

    public IParameter GetParameter( IParameterSymbol parameterSymbol )
        => (IParameter) this._symbolCache.GetOrAdd(
            parameterSymbol,
            static ( ms, c ) => new Parameter( (IParameterSymbol) ms, c ),
            this._compilationModel );

    public IEvent GetEvent( IEventSymbol eventSymbol )
        => (IEvent) this._symbolCache.GetOrAdd(
            eventSymbol,
            static ( ms, c ) => new Event( (IEventSymbol) ms, c ),
            this._compilationModel );

    public bool TryGetDeclaration( ISymbol symbol, [NotNullWhen( true )] out IDeclaration? declaration )
    {
        var compilationElement = this.GetCompilationElement( symbol );
        declaration = compilationElement as IDeclaration;

        return declaration != null;
    }

    internal IDeclaration? GetDeclarationOrNull( ISymbol symbol, DeclarationRefTargetKind kind = DeclarationRefTargetKind.Default )
        => this.GetCompilationElement( symbol, kind ) as IDeclaration;

    public IDeclaration GetDeclaration( SymbolDictionaryKey key ) => this.GetDeclaration( key.GetSymbolId().Resolve( this.Compilation ).AssertSymbolNotNull() );

    public IDeclaration GetDeclaration( ISymbol symbol ) => this.GetDeclaration( symbol, DeclarationRefTargetKind.Default );

    internal IDeclaration GetDeclaration( ISymbol symbol, DeclarationRefTargetKind kind )
    {
        var compilationElement = this.GetCompilationElement( symbol, kind );

        if ( compilationElement is not IDeclaration declaration )
        {
            throw new ArgumentException( $"{symbol.Kind} is not a declaration", nameof(symbol) );
        }

        return declaration;
    }

    internal ICompilationElement? GetCompilationElement( ISymbol symbol, DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
    {
        switch ( symbol.Kind )
        {
            case SymbolKind.Local:
            case SymbolKind.Label:
            case SymbolKind.ErrorType:
                return null;
        }

        symbol.ThrowIfBelongsToDifferentCompilationThan( this.CompilationContext );

        switch ( symbol.Kind )
        {
            case SymbolKind.NamedType:
                {
                    var type = this.GetNamedType( (INamedTypeSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.StaticConstructor => type.StaticConstructor,
                        DeclarationRefTargetKind.Default => type,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.ArrayType:
                return this.GetArrayType( (IArrayTypeSymbol) symbol );

            case SymbolKind.PointerType:
                return this.GetPointerType( (IPointerTypeSymbol) symbol );

            case SymbolKind.DynamicType:
                return this.GetDynamicType( (IDynamicTypeSymbol) symbol );

            case SymbolKind.Method:
                {
                    var method = (IMethodSymbol) symbol;

                    return
                        targetKind == DeclarationRefTargetKind.Return
                            ? this.GetReturnParameter( method )
                            : method.GetDeclarationKind() switch
                            {
                                DeclarationKind.Method => this.GetMethod( method ),
                                DeclarationKind.Constructor => this.GetConstructor( method ),
                                DeclarationKind.Finalizer => this.GetFinalizer( method ),
                                _ => throw new AssertionFailedException( $"Unexpected DeclarationRefTargetKind: {method.GetDeclarationKind()}." )
                            };
                }

            case SymbolKind.Property:
                var propertySymbol = (IPropertySymbol) symbol;

                var propertyOrIndexer = propertySymbol.IsIndexer
                    ? (IPropertyOrIndexer) this.GetIndexer( propertySymbol )
                    : this.GetProperty( propertySymbol );

                return targetKind switch
                {
                    // Implicit getter or setter.
                    DeclarationRefTargetKind.PropertyGet => propertyOrIndexer.GetMethod,
                    DeclarationRefTargetKind.PropertySet => propertyOrIndexer.SetMethod,

                    // The property itself.
                    DeclarationRefTargetKind.Default => propertyOrIndexer,
                    DeclarationRefTargetKind.Property => propertyOrIndexer,
                    _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                };

            case SymbolKind.Field:
                {
                    var field = this.GetField( (IFieldSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.Default => field,
                        DeclarationRefTargetKind.Field => field,
                        DeclarationRefTargetKind.PropertyGet => field.GetMethod,
                        DeclarationRefTargetKind.PropertySet => field.SetMethod,
                        DeclarationRefTargetKind.PropertyGetReturnParameter => field.GetMethod?.ReturnParameter,
                        DeclarationRefTargetKind.PropertySetParameter => field.SetMethod?.Parameters[0],
                        DeclarationRefTargetKind.PropertySetReturnParameter => field.SetMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.TypeParameter:
                return this.GetGenericParameter( (ITypeParameterSymbol) symbol );

            case SymbolKind.Parameter:
                return this.GetParameter( (IParameterSymbol) symbol );

            case SymbolKind.Event:
                {
                    var @event = this.GetEvent( (IEventSymbol) symbol );

                    return targetKind switch
                    {
                        DeclarationRefTargetKind.Default => @event,
                        DeclarationRefTargetKind.EventRaise => @event.RaiseMethod,
                        DeclarationRefTargetKind.EventRaiseParameter => throw new NotImplementedException(),
                        DeclarationRefTargetKind.EventRaiseReturnParameter => @event.RaiseMethod?.ReturnParameter,
                        _ => throw new AssertionFailedException( $"Invalid DeclarationRefTargetKind: {targetKind}." )
                    };
                }

            case SymbolKind.Assembly:
                return this.GetAssembly( (IAssemblySymbol) symbol );

            case SymbolKind.Namespace:
                return this.GetNamespace( (INamespaceSymbol) symbol );

            case SymbolKind.NetModule:
                return this._compilationModel;

            default:
                throw new AssertionFailedException( $"Don't know how to resolve a '{symbol.Kind}'." );
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